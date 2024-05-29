using System.Text.Json;
using Fora.ImportService.Models;
using Microsoft.Extensions.Logging;

namespace Fora.ImportService.SecEdgar;

public class SecEdgarClient(IHttpClientFactory httpClientFactory, ILogger<SecEdgarClient> logger)
    : ISecEdgarClient
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<SecEdgarClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<EdgarCompanyInfo> GetCompanyDataAsync(string cik)
    {
        if (string.IsNullOrWhiteSpace(cik))
        {
            throw new ArgumentException("CIK must be provided", nameof(cik));
        }

        var httpClient = _httpClientFactory.CreateClient(nameof(SecEdgarClient));
        httpClient.BaseAddress = new Uri(httpClient.BaseAddress, "companyfacts/");
        var endpoint = $"CIK{cik.PadLeft(10, '0')}.json";
        try
        {
            var response = await httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve data for CIK {CIK}: HTTP {StatusCode}", cik, response.StatusCode);
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var companyInfo = JsonSerializer.Deserialize<EdgarCompanyInfo>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return companyInfo;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching data for CIK: {CIK}", cik);
            throw;
        }
    }

    public async Task<EdgarCompanyInfo> GetCompanyDataAsync(int cik)
    {
        return await GetCompanyDataAsync(cik.ToString());
    }
}