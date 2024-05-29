using Fora.Service.Funding.ChainHandlers.Contexts;
using Microsoft.Extensions.Logging;

namespace Fora.Service.Funding.Strategies;

public class SpecialFundingCalculationStrategy(ILogger<SpecialFundingCalculationStrategy> logger) : ISpecialFundingCalculationStrategy
{
    public async Task<decimal> CalculateAsync(FundingHandlerContext context)
    {
        return await Task.Run(() =>
        {
            if (context == null)
            {
                logger.LogError("Failed to calculate special fundable amount: context is null.");
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.IsIncomeValidated || context.StandardFundableAmount == 0)
            {
                logger.LogInformation("Income data failed validation or standard fundable amount is zero.");
                return 0;
            }

            var specialFundableAmount = context.StandardFundableAmount;
            const string vowels = "AEIOUaeiou";

            if (!string.IsNullOrEmpty(context.CompanyInfo.EntityName) &&
                vowels.Contains(context.CompanyInfo.EntityName[0]))
            {
                specialFundableAmount += context.StandardFundableAmount * 0.15m; // Add 15% of the original standard amount to the special amount
                logger.LogInformation(
                    $"Increased fundable amount by 15% for starting with a vowel: {context.CompanyInfo.EntityName}");
            }

            try
            {
                var incomeYear2021 = context.CompanyInfo.Facts.UsGaap.NetIncomeLoss.Units
                    .SelectMany(unit => unit.Usd)
                    .FirstOrDefault(ir => ir.Frame == "CY2021")?.Val ?? 0;

                var incomeYear2022 = context.CompanyInfo.Facts.UsGaap.NetIncomeLoss.Units
                    .SelectMany(unit => unit.Usd)
                    .FirstOrDefault(ir => ir.Frame == "CY2022")?.Val ?? 0;

                if (incomeYear2022 < incomeYear2021)
                {
                    specialFundableAmount -= context.StandardFundableAmount * 0.25m; // Subtract 25% of the original standard amount from the special amount
                    logger.LogInformation(
                        "Decreased fundable amount by 25% due to decreased income year over year between years of concern.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while calculating the special fundable amount.");
                throw;
            }

            return specialFundableAmount;
        });
    }
}