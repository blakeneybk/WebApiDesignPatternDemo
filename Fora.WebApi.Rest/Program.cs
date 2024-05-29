using System.Net.Http.Headers;
using AutoMapper;
using Fora.Api;
using Fora.Data;
using Fora.Data.LiteDB;
using Fora.Data.Models;
using Fora.ImportService.Interfaces;
using Fora.ImportService.SecEdgar;
using Fora.Service.CompanyService;
using Fora.Service.Funding;
using Fora.Service.Funding.ChainHandlers;
using Fora.Service.Funding.Factories;
using Fora.Service.Funding.Strategies;

namespace Fora.WebApi.Rest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Configure logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAutoMapper(typeof(MappingProfile));
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new MappingProfile());
            });

            mapperConfiguration.AssertConfigurationIsValid();

            builder.Services.AddHttpClient(nameof(SecEdgarClient), client =>
            {
                client.BaseAddress = new Uri("https://data.sec.gov/api/xbrl/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.34.0");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            });

            builder.Services.AddSingleton<ISecEdgarClient, SecEdgarClient>();
            builder.Services.AddSingleton<ISeedDataProvider, SecEdgarSeedDataProvider>();
            var liteDbConnectionString = builder.Configuration.GetConnectionString("LiteDbConnection");
            builder.Services.AddSingleton<IRepository<CompanyInfo>>(provider => new LiteDbRepository<CompanyInfo>(liteDbConnectionString));
            builder.Services.AddSingleton<IDatabaseProvisioner>(provider => new LiteDbProvisioner(
                provider.GetRequiredService<ISeedDataProvider>(),
                provider.GetRequiredService<IRepository<CompanyInfo>>(),
                provider.GetRequiredService<IMapper>(),
                liteDbConnectionString
            ));
            builder.Services.AddHostedService<DatabaseProvisioningService>();

            // strategy
            builder.Services.AddScoped<IFundingCalculationStrategyFactory, FundingCalculationStrategyFactory>();
            builder.Services.AddScoped<IValidationStrategyFactory, ValidationStrategyFactory>();
            builder.Services.AddTransient<IStandardIncomeValidationStrategy, StandardIncomeValidationStrategy>();
            builder.Services.AddTransient<IStandardFundingCalculationStrategy, StandardFundingCalculationStrategy>();
            builder.Services.AddTransient<ISpecialFundingCalculationStrategy, SpecialFundingCalculationStrategy>();

            // chain of responsibility handlers
            builder.Services.AddTransient<IEntryFundingCalculationHandler, EntryFundingCalculationHandler>();
            builder.Services.AddTransient<IIncomeDataValidationHandler, IncomeDataValidationHandler>();
            builder.Services.AddTransient<IStandardCalculationHandler, StandardCalculationHandler>();
            builder.Services.AddTransient<ISpecialCalculationHandler, SpecialCalculationHandler>();

            // Register the services
            builder.Services.AddScoped<IFundingService, FundingService>();
            builder.Services.AddScoped<ICompanyService, CompanyService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
