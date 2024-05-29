using Fora.Service.Funding.ChainHandlers.Base;
using Fora.Service.Funding.ChainHandlers.Contexts;
using Fora.Service.Funding.Factories;
using Fora.Service.Funding.Strategies.StrategyTypes;
using Microsoft.Extensions.Logging;

namespace Fora.Service.Funding.ChainHandlers;

public class IncomeDataValidationHandler(
    IValidationStrategyFactory strategyFactory,
    ILogger<IncomeDataValidationHandler> logger)
    : FundingHandlerBase, IIncomeDataValidationHandler
{
    public override async Task<FundingHandlerContext> HandleAsync(FundingHandlerContext context)
    {
        try
        {
            logger.LogInformation($"{nameof(IncomeDataValidationHandler)} - handling executed.");
            var strategy = strategyFactory.GetStrategy(ValidationStrategyType.StandardIncome);
            context.IsIncomeValidated = await strategy.ValidateAsync(context);
            return _nextHandler != null ? await _nextHandler.HandleAsync(context) : context;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(IncomeDataValidationHandler)} - Exception: {ex}");
            context.IsValidContextResult = false;
            return context;
        }
    }
}