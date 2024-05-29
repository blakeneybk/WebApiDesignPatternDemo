using AutoMapper;
using Fora.Data;
using Fora.Data.Models;
using Fora.Service.Funding;
using Fora.Service.Funding.ChainHandlers.Contexts;
using Microsoft.Extensions.Logging;

namespace Fora.Service.CompanyService
{
    public class CompanyService(IRepository<CompanyInfo> repository, IFundingService fundingService, IMapper mapper, ILogger<CompanyService> logger) : ICompanyService
    {
        public async Task<IEnumerable<CompanyDto>> GetCompaniesByPrefix(string startsWith)
        {
            var companies = await repository.FindAsync(c => c.EntityName.StartsWith(startsWith));
            return mapper.Map<IEnumerable<CompanyDto>>(companies);
        }

        public async Task<IEnumerable<CompanyDto>> GetCompanies()
        {
            var companies = await repository.GetAllAsync();
            return mapper.Map<IEnumerable<CompanyDto>>(companies);
        }

        public async Task<IEnumerable<CompanyFundingDto>> GetCompanyFunding(IEnumerable<CompanyDto> companyInfos)
        {
            var rv = new List<CompanyFundingDto>();
            var processor = fundingService.GetFundingChain();
            foreach (var company in companyInfos)
            {
                var rvContext = await processor.HandleAsync(new FundingHandlerContext(mapper.Map<CompanyInfo>(company)));
                if (rvContext.IsValidContextResult)
                {
                    rv.Add(mapper.Map<CompanyFundingDto>(rvContext));
                }
                else
                {
                    logger.LogCritical($"Funding calculation processing failed for company ID: {company.Id} and wasn't added to the output. See exception logs.");
                }
            }

            return rv.Distinct();
        }
    }
}
