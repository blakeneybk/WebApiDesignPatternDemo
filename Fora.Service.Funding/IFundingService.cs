using Fora.Service.Funding.ChainHandlers.Base;

namespace Fora.Service.Funding;

public interface IFundingService
{
    IFundingHandler GetFundingChain();
}