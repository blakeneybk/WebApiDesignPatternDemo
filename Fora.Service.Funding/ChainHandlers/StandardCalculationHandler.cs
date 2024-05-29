using Fora.Service.Funding.ChainHandlers.Base;
using Fora.Service.Funding.ChainHandlers.Contexts;
using Fora.Service.Funding.Factories;
using Fora.Service.Funding.Strategies.StrategyTypes;
using Microsoft.Extensions.Logging;

namespace Fora.Service.Funding.ChainHandlers;

public class StandardCalculationHandler(
    IFundingCalculationStrategyFactory strategyFactory,
    ILogger<StandardCalculationHandler> logger)
    : FundingHandlerBase, IStandardCalculationHandler
{
    public override async Task<FundingHandlerContext> HandleAsync(FundingHandlerContext context)
    {
        try
        {
            logger.LogInformation($"{nameof(StandardCalculationHandler)} - Handling execution started.");
            var strategy = strategyFactory.GetStrategy(FundingStrategyType.Standard);
            context.StandardFundableAmount = await strategy.CalculateAsync(context);
            logger.LogInformation($"{nameof(StandardCalculationHandler)} - Calculation completed; Standard Fundable Amount: {context.StandardFundableAmount}");
            return _nextHandler != null ? await _nextHandler.HandleAsync(context) : context;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{nameof(StandardCalculationHandler)} - Exception occurred during fund calculation.");
            context.IsValidContextResult = false;
            return context;
        }
    }
}