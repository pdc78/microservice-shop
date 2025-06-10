using Azure.Core;
using Azure.Messaging.ServiceBus;
using InventoryService.Application.Interfaces;
using InventoryService.Infrastructure.Services;
using InventoryService.Infrastructure.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

IHost host = Host.CreateDefaultBuilder(args)
.ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        var serviceBusConnectionString = configuration.GetConnectionString("ServiceBus");
        var topicName = configuration.GetSection("Topics__Inventory");

        if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
            throw new InvalidOperationException("Service Bus connection string is not configured.");

        services.AddSingleton(sp =>
        {
            var serviceBusClientOption = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(0.5),
                    MaxDelay = TimeSpan.FromSeconds(10)
                }
            };

            return new ServiceBusClient(serviceBusConnectionString, serviceBusClientOption);

        });

        services.AddScoped<IInventoryService>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
            var logger = sp.GetRequiredService<ILogger<InventoryProcessorService>>();

            var topicName = configuration["Topics:Inventory"];
            if (string.IsNullOrWhiteSpace(topicName))
                throw new InvalidOperationException("Missing Inventory topic configuration");

            return new InventoryProcessorService(serviceBusClient, topicName, logger);
        });


        services.AddHostedService<InventoryWorker>();

        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

await host.RunAsync();
