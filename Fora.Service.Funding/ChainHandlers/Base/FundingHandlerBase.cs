using Fora.Service.Funding.ChainHandlers.Contexts;

namespace Fora.Service.Funding.ChainHandlers.Base
{
    public abstract class FundingHandlerBase : IFundingHandler
    {
        protected IFundingHandler _nextHandler;

        public IFundingHandler SetNext(IFundingHandler handler)
        {
            _nextHandler = handler;
            return _nextHandler;
        }

        public abstract Task<FundingHandlerContext> HandleAsync(FundingHandlerContext context);
    }
}
