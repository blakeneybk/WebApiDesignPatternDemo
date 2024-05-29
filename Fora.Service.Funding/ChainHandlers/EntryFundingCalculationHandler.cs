using Fora.Service.Funding.ChainHandlers.Base;
using Fora.Service.Funding.ChainHandlers.Contexts;

namespace Fora.Service.Funding.ChainHandlers;

public sealed class EntryFundingCalculationHandler : FundingHandlerBase, IEntryFundingCalculationHandler
{
    public override async Task<FundingHandlerContext> HandleAsync(FundingHandlerContext context)
    {
        if (_nextHandler == null)
        {
            context.IsValidContextResult = false;
            return context;
        }
        return await _nextHandler.HandleAsync(context);
    }
}