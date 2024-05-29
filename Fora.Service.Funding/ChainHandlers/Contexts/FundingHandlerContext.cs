using Fora.Data.Models;

namespace Fora.Service.Funding.ChainHandlers.Contexts;

public class FundingHandlerContext(CompanyInfo companyInfo)
{
    public CompanyInfo CompanyInfo { get; private set; } = companyInfo;
    public bool IsIncomeValidated { get; set; }
    public decimal StandardFundableAmount { get; set; } = 0;
    public decimal SpecialFundableAmount { get; set; } = 0;
    public bool IsValidContextResult { get; set; } = true;
}