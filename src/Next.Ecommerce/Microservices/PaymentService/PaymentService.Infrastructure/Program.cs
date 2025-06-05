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
        {
            throw new InvalidOperationException("Service Bus connection string is not configured.");
        }

        services.AddSingleton(new ServiceBusClient(serviceBusConnectionString));
        services.AddScoped<IPaymentService, PaymentProcessorService>();

        services.AddHostedService<PaymentWorker>();

        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });
    })
    .Build();

await host.RunAsync();
