using Fora.Service.Funding.ChainHandlers.Contexts;
using Microsoft.Extensions.Logging;

namespace Fora.Service.Funding.Strategies;

public class StandardIncomeValidationStrategy(ILogger<StandardIncomeValidationStrategy> logger) : IStandardIncomeValidationStrategy
{
    public async Task<bool> ValidateAsync(FundingHandlerContext context)
    {
        return await Task.Run(() =>
        {
            if (context == null)
            {
                logger.LogError("Validation failed: context is null.");
                throw new ArgumentNullException(nameof(context));
            }

            try
            {
                var yearsRequired = new[] { "CY2018", "CY2019", "CY2020", "CY2021", "CY2022" };
                var yearsRequiredPositive = new[] { "CY2021", "CY2022" };

                if (context.CompanyInfo?.Facts?.UsGaap?.NetIncomeLoss?.Units == null)
                {
                    logger.LogInformation("Validation failed: NetIncomeLoss units are null.");
                    return false;
                }

                var incomeRecords = context.CompanyInfo.Facts.UsGaap.NetIncomeLoss.Units
                    .SelectMany(unit => unit.Usd).ToList();

                // Check for income data across all required years
                var hasDataForAllRequiredYears = yearsRequired.All(year =>
                    incomeRecords.Any(ir => ir.Frame == year));

                // Ensure positive income in required years
                var hasPositiveIncomeForRequiredYears = yearsRequiredPositive.All(year =>
                    incomeRecords.Any(ir => ir.Frame == year && ir.Val > 0));

                if (!hasDataForAllRequiredYears)
                {
                    logger.LogInformation("Validation failed: Not all required years have income data.");
                }

                if (!hasPositiveIncomeForRequiredYears)
                {
                    logger.LogInformation("Validation failed: Not all required years have positive income.");
                }

                return hasDataForAllRequiredYears && hasPositiveIncomeForRequiredYears;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred during income validation strategy.");
                throw;
            }
        });
    }
}