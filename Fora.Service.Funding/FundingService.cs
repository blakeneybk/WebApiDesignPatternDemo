using Fora.Service.Funding.ChainHandlers.Base;
using Fora.Service.Funding.ChainHandlers;

namespace Fora.Service.Funding;

public class FundingService(
    IEntryFundingCalculationHandler entryHandler,
    IIncomeDataValidationHandler incomeHandler,
    IStandardCalculationHandler standardHandler,
    ISpecialCalculationHandler specialHandler)
    : IFundingService
{
    private IFundingHandler _chainHead;

    public IFundingHandler GetFundingChain()
    {
        return _chainHead ??= ConfigureHandlers();
    }

    private IFundingHandler ConfigureHandlers()
    {
        entryHandler.SetNext(incomeHandler).SetNext(standardHandler).SetNext(specialHandler);
        return entryHandler;
    }
}