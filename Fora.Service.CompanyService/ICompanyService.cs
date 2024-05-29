namespace Fora.Service.CompanyService
{
    public interface ICompanyService
    {
        public Task<IEnumerable<CompanyDto>> GetCompaniesByPrefix(string startsWith);
        public Task<IEnumerable<CompanyDto>> GetCompanies();
        public Task<IEnumerable<CompanyFundingDto>> GetCompanyFunding(IEnumerable<CompanyDto> companyInfos);
    }
}
