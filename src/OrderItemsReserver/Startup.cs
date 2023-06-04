using System;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderItemsReserver;

[assembly: FunctionsStartup(typeof(Startup))]

namespace OrderItemsReserver;

public class Startup : FunctionsStartup
{
    private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("AppSettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton((s) =>
        {
            var connectionString = _configuration["CosmosDbConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(connectionString);
            }

            CosmosClientBuilder configurationBuilder = new(connectionString);

            return configurationBuilder
                    .Build();
        });
    }
}
