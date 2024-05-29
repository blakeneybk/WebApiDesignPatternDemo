using Fora.Data.Models;
using Fora.Service.Funding.ChainHandlers.Contexts;
using Fora.Service.Funding.Strategies;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Fora.WebApi.Test;

[TestFixture]
public class SpecialFundingCalculationStrategyTests
{
    private Mock<ILogger<SpecialFundingCalculationStrategy>> _mockLogger;
    private SpecialFundingCalculationStrategy _strategy;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<SpecialFundingCalculationStrategy>>();
        _strategy = new SpecialFundingCalculationStrategy(_mockLogger.Object);
    }

    [Test]
    public void ValidateAsync_ContextIsNull_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _strategy.CalculateAsync(null));
    }

    [Test]
    public async Task CalculateAsync_IncomeDataFailedValidation_ReturnsZero()
    {
        var context = CreateMockContext(new Dictionary<string, decimal> { { "CY2021", 1000000 } }, "Apple", false);
        var result = await _strategy.CalculateAsync(context);
        Assert.AreEqual(0, result);
    }

    [Test]
    public async Task CalculateAsync_StandardFundableAmountIsZero_ReturnsZero()
    {
        var context = CreateMockContext(new Dictionary<string, decimal> { { "CY2021", 1000000 } }, "Apple", true);
        context.StandardFundableAmount = 0;
        var result = await _strategy.CalculateAsync(context);
        Assert.AreEqual(0, result);
    }

    [Test]
    public async Task CalculateAsync_CompanyNameStartsWithVowel_IncreasesFundableAmount()
    {
        var context = CreateMockContext(new Dictionary<string, decimal> { { "CY2021", 500000 }, { "CY2022", 600000 } }, "Amazon", true);
        var result = await _strategy.CalculateAsync(context);
        Assert.AreEqual(context.StandardFundableAmount * 1.15m, result);
    }

    [Test]
    public async Task CalculateAsync_IncomeDecrease_SubtractsFromFundableAmount()
    {
        var context = CreateMockContext(new Dictionary<string, decimal> { { "CY2021", 600000 }, { "CY2022", 500000 } }, "Facebook", true);
        var result = await _strategy.CalculateAsync(context);
        Assert.AreEqual(context.StandardFundableAmount * 0.75m, result);
    }

    [Test]
    public async Task CalculateAsync_VowelAndIncomeDecrease_AppliesBothConditions()
    {
        var context = CreateMockContext(new Dictionary<string, decimal> { { "CY2021", 600000 }, { "CY2022", 500000 } }, "Oracle", true);
        var result = await _strategy.CalculateAsync(context);
        // First increase by 15% then decrease by 25%
        var sfa = context.StandardFundableAmount;
        sfa += context.StandardFundableAmount * 0.15m;
        sfa -= context.StandardFundableAmount * 0.25m;
        var expected = sfa;
        Assert.AreEqual(expected, result);
    }

    private FundingHandlerContext CreateMockContext(Dictionary<string, decimal> yearlyIncomes, string companyName, bool isIncomeValidated)
    {
        var units = yearlyIncomes.Select(kvp => new InfoFactUsGaapIncomeLossUnitsUsd
        {
            Form = "10-K",
            Frame = kvp.Key,
            Val = kvp.Value
        }).ToList();

        var companyInfo = new CompanyInfo
        {
            EntityName = companyName,
            Facts = new InfoFact
            {
                UsGaap = new InfoFactUsGaap
                {
                    NetIncomeLoss = new InfoFactUsGaapNetIncomeLoss
                    {
                        Units = new List<InfoFactUsGaapIncomeLossUnits> { new InfoFactUsGaapIncomeLossUnits { Usd = units } }
                    }
                }
            }
        };

        return new FundingHandlerContext(companyInfo)
        {
            IsIncomeValidated = isIncomeValidated,
            StandardFundableAmount = 1000000,
            SpecialFundableAmount = 0
        };
    }
}
