using AutoMapper;
using Fora.Data.Models;
using Fora.ImportService.Interfaces;
using LiteDB;

namespace Fora.Data.LiteDB;

public class LiteDbProvisioner(
    ISeedDataProvider seedDataProvider,
    IRepository<CompanyInfo> repository,
    IMapper mapper,
    string connectionString)
    : IDatabaseProvisioner
{
    public async Task ProvisionDatabaseAsync()
    {
        DeleteDatabase();

        var seedData = await seedDataProvider.GetSeedDataAsync();
        var companyInfos = mapper.Map<IEnumerable<CompanyInfo>>(seedData);

        foreach (var companyInfo in companyInfos)
        {
            await repository.AddAsync(companyInfo);
        }
    }

    // Wipe the database file to start with fresh provisioning data
    private void DeleteDatabase()
    {
        var dbPath = new ConnectionString(connectionString).Filename;
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
            Console.WriteLine("Existing database deleted successfully.");
        }
    }
}