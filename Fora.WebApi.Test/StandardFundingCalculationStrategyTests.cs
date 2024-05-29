using Fora.Data.Models;
using Fora.Service.Funding.ChainHandlers.Contexts;
using Fora.Service.Funding.Strategies;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Fora.WebApi.Test;

[TestFixture]
public class StandardFundingCalculationStrategyTests
{
    private Mock<ILogger<StandardFundingCalculationStrategy>> _mockLogger;
    private StandardFundingCalculationStrategy _strategy;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<StandardFundingCalculationStrategy>>();
        _strategy = new StandardFundingCalculationStrategy(_mockLogger.Object);
    }

    [Test]
    public void ValidateAsync_ContextIsNull_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _strategy.CalculateAsync(null));
    }

    [Test]
    public async Task CalculateAsync_IncomeDataFailedValidation_ReturnsZero()
    {
        var context = CreateMockContext(new Dictionary<string, decimal>(), false);
        var result = await _strategy.CalculateAsync(context);
        Assert.AreEqual(0, result);
    }

    [Test]
    public async Task CalculateAsync_HighIncome_CorrectCalculation()
    {
        var context = CreateMockContext(new Dictionary<string, decimal> { {"CY2021", 11000000000m} }, true);
        var result = await _strategy.CalculateAsync(context);
        Assert.AreEqual(11000000000m * 0.1233m, result);
    }

    [Test]
    public async Task CalculateAsync_NormalIncome_CorrectCalculation()
    {
        var context = CreateMockContext(new Dictionary<string, decimal> { {"CY2021", 5000000000m} }, true);
        var result = await _strategy.CalculateAsync(context);
        Assert.AreEqual(5000000000m * 0.2151m, result);
    }

    private FundingHandlerContext CreateMockContext(Dictionary<string, decimal> yearlyIncomes, bool isIncomeValidated)
    {
        var units = yearlyIncomes.Select(kvp => new InfoFactUsGaapIncomeLossUnitsUsd
        {
            Form = "10-K",
            Frame = kvp.Key,
            Val = kvp.Value
        }).ToList();

        var companyInfo = new CompanyInfo
        {
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
            StandardFundableAmount = 0,
            SpecialFundableAmount = 0,
            IsValidContextResult = true
        };
    }
}