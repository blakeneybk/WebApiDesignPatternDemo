using Fora.Service.Funding.Strategies;
using Fora.Service.Funding.Strategies.Base;
using Fora.Service.Funding.Strategies.StrategyTypes;

namespace Fora.Service.Funding.Factories;

public interface IFundingCalculationStrategyFactory
{
    IFundingCalculationStrategy GetStrategy(FundingStrategyType type);
}