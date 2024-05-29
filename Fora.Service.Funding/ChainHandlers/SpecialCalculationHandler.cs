using Fora.Service.Funding.ChainHandlers.Base;
using Fora.Service.Funding.ChainHandlers.Contexts;
using Fora.Service.Funding.Factories;
using Fora.Service.Funding.Strategies.StrategyTypes;
using Microsoft.Extensions.Logging;

namespace Fora.Service.Funding.ChainHandlers;

public class SpecialCalculationHandler(
    IFundingCalculationStrategyFactory strategyFactory,
    ILogger<SpecialCalculationHandler> logger)
    : FundingHandlerBase, ISpecialCalculationHandler
{
    public override async Task<FundingHandlerContext> HandleAsync(FundingHandlerContext context)
    {
        try
        {
            logger.LogInformation($"{nameof(SpecialCalculationHandler)} - Handling execution started.");
            var strategy = strategyFactory.GetStrategy(FundingStrategyType.Special);
            context.SpecialFundableAmount = await strategy.CalculateAsync(context);
            logger.LogInformation($"{nameof(SpecialCalculationHandler)} - Calculation completed; Special Fundable Amount: {context.SpecialFundableAmount}");
            return _nextHandler != null ? await _nextHandler.HandleAsync(context) : context;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(SpecialCalculationHandler)} - Exception occurred during special fund calculation.");
            context.IsValidContextResult = false;
            return context;
        }
    }
}