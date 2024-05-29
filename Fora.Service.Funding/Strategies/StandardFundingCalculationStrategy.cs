using Fora.Service.Funding.ChainHandlers.Contexts;
using Microsoft.Extensions.Logging;

namespace Fora.Service.Funding.Strategies;

public class StandardFundingCalculationStrategy(ILogger<StandardFundingCalculationStrategy> logger) : IStandardFundingCalculationStrategy
{
    public async Task<decimal> CalculateAsync(FundingHandlerContext context)
    {
        try
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return await Task.Run(() =>
            {
                var yearsRequired = new[] { "CY2018", "CY2019", "CY2020", "CY2021", "CY2022" };

                if (!context.IsIncomeValidated)
                {
                    logger.LogInformation("Income data failed validation returning zero.");
                    return 0;
                }

                var incomeRecords = context.CompanyInfo.Facts.UsGaap.NetIncomeLoss.Units
                    .SelectMany(unit => unit.Usd)
                    .Where(ir => yearsRequired.Contains(ir.Frame))
                    .ToList();

                var highestIncome = incomeRecords.Max(ir => ir.Val);
                var fundablePercentage = highestIncome >= 10000000000m ? 0.1233m : 0.2151m;
                var standardFundableAmount = highestIncome * fundablePercentage;

                logger.LogInformation($"Calculated standard fundable amount: {standardFundableAmount}");
                return standardFundableAmount;
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during the calculation of the standard fundable amount.");
            throw;
        }
    }
}