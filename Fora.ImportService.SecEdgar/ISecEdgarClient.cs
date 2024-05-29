using Fora.ImportService.Models;

namespace Fora.ImportService.SecEdgar
{
    public interface ISecEdgarClient
    {
        Task<EdgarCompanyInfo> GetCompanyDataAsync(string cik);
        Task<EdgarCompanyInfo> GetCompanyDataAsync(int cik);
    }
}