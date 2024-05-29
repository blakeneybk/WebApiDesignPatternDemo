using Fora.Service.Funding.Strategies;
using Fora.Service.Funding.Strategies.Base;
using Fora.Service.Funding.Strategies.StrategyTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Fora.Service.Funding.Factories;

public class FundingCalculationStrategyFactory(IServiceProvider serviceProvider) : IFundingCalculationStrategyFactory
{
    public IFundingCalculationStrategy GetStrategy(FundingStrategyType type)
    {
        return type switch
        {
            FundingStrategyType.Standard => serviceProvider.GetRequiredService<IStandardFundingCalculationStrategy>(),
            FundingStrategyType.Special => serviceProvider.GetRequiredService<ISpecialFundingCalculationStrategy>(),
            _ => throw new ArgumentException($"Unsupported strategy type: {type}")
        };
    }
}