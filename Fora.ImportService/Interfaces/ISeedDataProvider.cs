using Fora.ImportService.Models;

namespace Fora.ImportService.Interfaces;

public interface ISeedDataProvider
{
    Task<IEnumerable<EdgarCompanyInfo>> GetSeedDataAsync();
}