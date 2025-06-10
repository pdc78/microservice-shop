using Azure.Messaging.ServiceBus;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Services;
using PaymentService.Infrastructure.Workers;
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
            throw new InvalidOperationException("Missing Service Bus connection string");

        services.AddSingleton(sp =>
        {
            var clientOption = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(0.5),
                    MaxDelay = TimeSpan.FromSeconds(10)
                }
            };
            return new ServiceBusClient(serviceBusConnectionString, clientOption);
        });

        services.AddScoped<IPaymentService>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
            var logger = sp.GetRequiredService<ILogger<PaymentProcessorService>>();

            var topicName = configuration["Topics:Payment"];
            if (string.IsNullOrWhiteSpace(topicName))
                throw new InvalidOperationException("Missing Payment topic configuration");

            return new PaymentProcessorService(serviceBusClient, topicName, logger);
        });

        services.AddHostedService<PaymentWorker>();

        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

await host.RunAsync();
