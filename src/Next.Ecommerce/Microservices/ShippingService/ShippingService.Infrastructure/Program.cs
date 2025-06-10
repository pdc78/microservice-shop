using Azure.Messaging.ServiceBus;
using ShippingService.Application.Interfaces;
using ShippingService.Infrastructure.Services;
using ShippingService.Infrastructure.Workers;
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

        if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
            throw new InvalidOperationException("Service Bus connection string is not configured.");

        services.AddSingleton(sp =>
        {
            var serviceBusClientOption = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode= ServiceBusRetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(0.5),
                    MaxDelay= TimeSpan.FromSeconds(10)
                }
            };

            return new ServiceBusClient(serviceBusConnectionString, serviceBusClientOption);
        });


        services.AddScoped<IShippingService>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
            var logger = sp.GetRequiredService<ILogger<ShippingProcessorService>>();

            var topicName = configuration["Topics:Shipping"];
            if (string.IsNullOrWhiteSpace(topicName))
                throw new InvalidOperationException("Missing Shipping topic configuration");

            return new ShippingProcessorService(serviceBusClient, topicName, logger);
        });

        services.AddHostedService<ShippingWorker>();

        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

await host.RunAsync();
