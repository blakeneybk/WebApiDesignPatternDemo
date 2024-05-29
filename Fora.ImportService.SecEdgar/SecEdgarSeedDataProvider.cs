using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Fora.ImportService.Interfaces;
using Fora.ImportService.Models;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace Fora.ImportService.SecEdgar;

public class SecEdgarSeedDataProvider : ISeedDataProvider, IDisposable
{
    private const int MAX_SEMAPHORE = 10;
    private static readonly int MAX_TASKS = (int)Math.Round(MAX_SEMAPHORE * 1.5);
    private readonly ISecEdgarClient _secEdgarClient;
    private readonly ILogger<SecEdgarSeedDataProvider> _logger;
    private readonly SemaphoreSlim _batchLimiter;
    private readonly Timer _rateLimiterTimer;
    private bool _disposed = false;

    public SecEdgarSeedDataProvider(ISecEdgarClient secEdgarClient, ILogger<SecEdgarSeedDataProvider> logger)
    {
        _secEdgarClient = secEdgarClient;
        _logger = logger;
        // The documented rate limit of SEC Edgar is 10 requests per second
        _batchLimiter = new SemaphoreSlim(MAX_SEMAPHORE, MAX_SEMAPHORE);
        _rateLimiterTimer = new Timer(1000); // Reset semaphore every second
        _rateLimiterTimer.Elapsed += OnTimerElapsed;
        _rateLimiterTimer.AutoReset = true;
        _rateLimiterTimer.Start();
    }

    public async Task<IEnumerable<EdgarCompanyInfo>> GetSeedDataAsync()
    {
        var responseBag = new ConcurrentBag<EdgarCompanyInfo>();
        List<Task> tasks = new List<Task>();

        foreach (int cik in Constants.CIK_DATA_TARGETS)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    await _batchLimiter.WaitAsync();

                    var companyData = await _secEdgarClient.GetCompanyDataAsync(cik);

                    if (companyData != null)
                    {
                        // Data santizing rules
                        // Add only if Form is "10-K" AND
                        // Add only if Frame is prefixed with CY and followed by 4 digits
                        var validEntries = companyData.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?
                            .Where(u => u.Form == "10-K" && u.Frame != null && Regex.IsMatch(u.Frame, @"^CY\d{4}$"))
                            .ToArray();

                        if (validEntries?.Any() == true)
                        {
                            companyData.Facts.UsGaap.NetIncomeLoss.Units.Usd = validEntries;

                            responseBag.Add(companyData);
                        }
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing CIK: {cik}");
                }
            });

            tasks.Add(task);

            // If we've hit the limit of concurrent tasks, wait for all to complete, allow a few tasks to wait in the wings for an open slot if the API is slow.
            if (tasks.Count == MAX_TASKS)
            {
                await Task.WhenAll(tasks);
                tasks.Clear(); // Clear the list to start the next batch
            }
        }

        // Ensure any remaining tasks are also completed
        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }

        return responseBag;
    }

    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            var releasesNeeded = MAX_SEMAPHORE - _batchLimiter.CurrentCount;
            if (releasesNeeded > 0)
            {
                _batchLimiter.Release(releasesNeeded);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release semaphore.");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _rateLimiterTimer?.Stop();
                _rateLimiterTimer?.Dispose();
                _batchLimiter?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}