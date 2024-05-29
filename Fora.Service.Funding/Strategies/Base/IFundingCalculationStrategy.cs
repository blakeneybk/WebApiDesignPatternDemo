using Fora.Service.Funding.ChainHandlers.Contexts;

namespace Fora.Service.Funding.Strategies.Base;

public interface IFundingCalculationStrategy
{
    Task<decimal> CalculateAsync(FundingHandlerContext income);
}