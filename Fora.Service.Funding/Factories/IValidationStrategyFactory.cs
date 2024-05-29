using Fora.Service.Funding.Strategies.Base;
using Fora.Service.Funding.Strategies.StrategyTypes;

namespace Fora.Service.Funding.Factories;

public interface IValidationStrategyFactory
{
    IValidationStrategy GetStrategy(ValidationStrategyType type);
}