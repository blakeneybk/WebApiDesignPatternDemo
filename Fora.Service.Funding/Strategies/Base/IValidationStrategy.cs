using Fora.Service.Funding.ChainHandlers.Contexts;

namespace Fora.Service.Funding.Strategies.Base;

public interface IValidationStrategy
{
    Task<bool> ValidateAsync(FundingHandlerContext companyInfo);
}