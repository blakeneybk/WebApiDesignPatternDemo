using Fora.Data.Models;
using Fora.Service.Funding;
using Fora.Service.Funding.ChainHandlers;
using Fora.Service.Funding.ChainHandlers.Base;
using Fora.Service.Funding.ChainHandlers.Contexts;
using Fora.Service.Funding.Factories;
using Fora.Service.Funding.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Fora.WebApi.Test;

[TestFixture]
public class FundingServiceChainIntegrationTests
{
    private IServiceProvider _serviceProvider;
    private Mock<ILogger<IncomeDataValidationHandler>> _mockIncomeLogger;
    private Mock<ILogger<StandardCalculationHandler>> _mockStandardLogger;
    private Mock<ILogger<SpecialCalculationHandler>> _mockSpecialLogger;
    private Mock<ILogger<StandardIncomeValidationStrategy>> _mockStandardIncomeStrategyLogger;
    private Mock<ILogger<StandardFundingCalculationStrategy>> _mockStandardFundingCalculationStrategyLogger;
    private Mock<ILogger<SpecialFundingCalculationStrategy>> _mockSpecialFundingCalculationStrategyLogger;

    [SetUp]
    public void SetUp()
    {
        _mockIncomeLogger = new Mock<ILogger<IncomeDataValidationHandler>>();
        _mockStandardLogger = new Mock<ILogger<StandardCalculationHandler>>();
        _mockSpecialLogger = new Mock<ILogger<SpecialCalculationHandler>>();

        _mockStandardIncomeStrategyLogger = new Mock<ILogger<StandardIncomeValidationStrategy>>();
        _mockStandardFundingCalculationStrategyLogger = new Mock<ILogger<StandardFundingCalculationStrategy>>();
        _mockSpecialFundingCalculationStrategyLogger = new Mock<ILogger<SpecialFundingCalculationStrategy>>();

        var services = new ServiceCollection();
        
        services.AddTransient<IStandardIncomeValidationStrategy>(provider => 
            new StandardIncomeValidationStrategy(_mockStandardIncomeStrategyLogger.Object));
        services.AddTransient<IStandardFundingCalculationStrategy>(provider => 
            new StandardFundingCalculationStrategy(_mockStandardFundingCalculationStrategyLogger.Object));
        services.AddTransient<ISpecialFundingCalculationStrategy>(provider => 
            new SpecialFundingCalculationStrategy(_mockSpecialFundingCalculationStrategyLogger.Object));

        services.AddSingleton<IValidationStrategyFactory>(provider => 
            new ValidationStrategyFactory(provider));
        services.AddSingleton<IFundingCalculationStrategyFactory>(provider => 
            new FundingCalculationStrategyFactory(provider));

        services.AddTransient<IEntryFundingCalculationHandler, EntryFundingCalculationHandler>(provider =>
            new EntryFundingCalculationHandler());
        services.AddTransient<IIncomeDataValidationHandler, IncomeDataValidationHandler>(provider =>
            new IncomeDataValidationHandler(provider.GetRequiredService<IValidationStrategyFactory>(), _mockIncomeLogger.Object));
        services.AddTransient<IStandardCalculationHandler, StandardCalculationHandler>(provider =>
            new StandardCalculationHandler(provider.GetRequiredService<IFundingCalculationStrategyFactory>(), _mockStandardLogger.Object));
        services.AddTransient<ISpecialCalculationHandler, SpecialCalculationHandler>(provider =>
            new SpecialCalculationHandler(provider.GetRequiredService<IFundingCalculationStrategyFactory>(), _mockSpecialLogger.Object));

        services.AddTransient<FundingService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestCase("Apple Inc.", new[] { 2018, 2019, 2020, 2021, 2022 }, new[] { 700000, 700000, 700000, 600000, 500000 }, true)]  // Vowel and decreasing
    [TestCase("X Corp", new[] { 2018, 2019, 2020, 2021, 2022 }, new[] { 700000, 700000, 700000, 700000, 700000 }, true)]  // No special conditions
    [TestCase("Fail Case", new[] { 2021, 2022 }, new[] { -1, -1 }, false)]  // Fails validation
    public async Task FundingService_HandlesVariousScenarios_Correctly(string companyName, int[] years, int[] values, bool shouldPass)
    {
        var fundingService = _serviceProvider.GetRequiredService<FundingService>();
        var chainHead = fundingService.GetFundingChain();

        var units = new List<InfoFactUsGaapIncomeLossUnitsUsd>();
        for (int i = 0; i < years.Length; i++)
        {
            units.Add(new InfoFactUsGaapIncomeLossUnitsUsd { Form = "10-K", Frame = $"CY{years[i]}", Val = values[i] });
        }

        var context = new FundingHandlerContext(new CompanyInfo
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
        });

        var resultContext = await chainHead.HandleAsync(context);

        Assert.IsNotNull(resultContext);
        if (shouldPass)
        {
            Assert.IsTrue(resultContext.IsValidContextResult);
            Assert.Greater(resultContext.StandardFundableAmount, 0);
            Assert.Greater(resultContext.SpecialFundableAmount, 0);
        }
        else
        {
            Assert.AreEqual(0, resultContext.StandardFundableAmount);
            Assert.AreEqual(0, resultContext.SpecialFundableAmount);
        }
    }

    [TestCase("X Corp", new[] { 2018, 2019, 2020, 2021, 2022 }, new[] { 700000, 700000, 700000, 700000, 700000 }, true)]  // No special conditions
    public async Task FundingService_Standard_Special_Same(string companyName, int[] years, int[] values, bool shouldPass)
    {
        var fundingService = _serviceProvider.GetRequiredService<FundingService>();
        var chainHead = fundingService.GetFundingChain();

        var units = new List<InfoFactUsGaapIncomeLossUnitsUsd>();
        for (int i = 0; i < years.Length; i++)
        {
            units.Add(new InfoFactUsGaapIncomeLossUnitsUsd { Form = "10-K", Frame = $"CY{years[i]}", Val = values[i] });
        }

        var context = new FundingHandlerContext(new CompanyInfo
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
        });

        var resultContext = await chainHead.HandleAsync(context);

        Assert.IsNotNull(resultContext);
        if (shouldPass)
        {
            Assert.IsTrue(resultContext.IsValidContextResult);
            Assert.AreEqual(resultContext.StandardFundableAmount, resultContext.SpecialFundableAmount);
        }
        else
        {
            Assert.AreEqual(0, resultContext.StandardFundableAmount);
            Assert.AreEqual(0, resultContext.SpecialFundableAmount);
        }
    }
}
