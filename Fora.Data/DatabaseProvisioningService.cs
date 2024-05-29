using Microsoft.Extensions.Hosting;

namespace Fora.Data;

public class DatabaseProvisioningService(IDatabaseProvisioner databaseProvisioner) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return databaseProvisioner.ProvisionDatabaseAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}