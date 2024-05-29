namespace Fora.Data;

public interface IDatabaseProvisioner
{
    Task ProvisionDatabaseAsync();
}