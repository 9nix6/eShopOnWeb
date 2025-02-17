﻿using System;
using System.Net.Http;
using DeliveryOrderProcessor;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace DeliveryOrderProcessor;

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
            string connectionString = _configuration["CosmosDbConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(connectionString);
            }

            CosmosClientBuilder configurationBuilder = new(connectionString);
            
            return configurationBuilder
                    .Build();
        });

        builder.Services.AddScoped(s => new HttpClient());
    }
}
