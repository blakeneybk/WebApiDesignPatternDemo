using Fora.Data.Models;
using Fora.Service.Funding.ChainHandlers.Contexts;
using Fora.Service.Funding.Strategies;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Fora.WebApi.Test;

[TestFixture]
public class StandardIncomeValidationStrategyTests
{
    private Mock<ILogger<StandardIncomeValidationStrategy>> _mockLogger;
    private StandardIncomeValidationStrategy _strategy;

    [SetUp]
    public void SetUp()
    {
        _mockLogger = new Mock<ILogger<StandardIncomeValidationStrategy>>();
        _strategy = new StandardIncomeValidationStrategy(_mockLogger.Object);
    }

    [Test]
    public void ValidateAsync_ContextIsNull_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() => _strategy.ValidateAsync(null));
    }

    [Test]
    public async Task ValidateAsync_NoIncomeData_ReturnsFalse()
    {
        var context = new FundingHandlerContext(new CompanyInfo
        {
            Facts = new InfoFact
            {
                UsGaap = new InfoFactUsGaap
                {
                    NetIncomeLoss = new InfoFactUsGaapNetIncomeLoss
                    {
                        Units = new List<InfoFactUsGaapIncomeLossUnits>()
                    }
                }
            }
        });

        var result = await _strategy.ValidateAsync(context);
        Assert.IsFalse(result);
    }

    private static IEnumerable<TestCaseData> IncomeValidationTestCases
    {
        get
        {
            yield return new TestCaseData(
                new Dictionary<string, decimal> { { "CY2018", 50000 }, { "CY2019", 60000 }, { "CY2020", 70000 }, { "CY2021", 80000 }, { "CY2022", 90000 } },
                true
            ).SetName("AllYearsPositiveIncome");
            yield return new TestCaseData(
                new Dictionary<string, decimal> { { "CY2018", 50000 }, { "CY2019", -10000 }, { "CY2020", 70000 }, { "CY2021", 80000 }, { "CY2022", -5000 } },
                false
            ).SetName("NegativeIncomeInRequiredYears");
            yield return new TestCaseData(
                new Dictionary<string, decimal> { { "CY2018", 50000 }, { "CY2021", 80000 }, { "CY2022", 5000 } },
                false
            ).SetName("MissingIncomeDataForRequiredYears");
        }
    }

    [TestCaseSource(nameof(IncomeValidationTestCases))]
    public async Task ValidateAsync_MultipleScenarios_ReturnsExpectedResults(Dictionary<string, decimal> yearlyIncomes, bool expected)
    {
        var context = CreateMockContext(yearlyIncomes);

        var result = await _strategy.ValidateAsync(context);

        Assert.AreEqual(expected, result);
    }

    private FundingHandlerContext CreateMockContext(Dictionary<string, decimal> yearlyIncomes)
    {
        var units = yearlyIncomes.Select(kvp => new InfoFactUsGaapIncomeLossUnitsUsd
        {
            Form = "10-K",
            Frame = kvp.Key,
            Val = kvp.Value
        }).ToList();

        return new FundingHandlerContext(new CompanyInfo
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
        });
    }
}