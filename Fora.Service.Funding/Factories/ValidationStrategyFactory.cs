using Fora.Service.Funding.Strategies;
using Fora.Service.Funding.Strategies.Base;
using Fora.Service.Funding.Strategies.StrategyTypes;
using Microsoft.Extensions.DependencyInjection;

namespace Fora.Service.Funding.Factories;

public class ValidationStrategyFactory(IServiceProvider serviceProvider) : IValidationStrategyFactory
{
    public IValidationStrategy GetStrategy(ValidationStrategyType type)
    {
        return type switch
        {
            ValidationStrategyType.StandardIncome => serviceProvider.GetRequiredService<IStandardIncomeValidationStrategy>(),
            _ => throw new ArgumentException($"Unsupported strategy type: {type}")
        };
    }
}