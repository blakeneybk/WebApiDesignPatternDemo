using Fora.Service.Funding.ChainHandlers.Contexts;

namespace Fora.Service.Funding.ChainHandlers.Base;

public interface IFundingHandler
{
    IFundingHandler SetNext(IFundingHandler handler);
    Task<FundingHandlerContext> HandleAsync(FundingHandlerContext context);
}