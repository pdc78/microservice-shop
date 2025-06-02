using System;
using System.Linq;
using System.Threading.Tasks;
using OrderService.Application.Interfaces;
using OrderService.Domain.DTOs;
using OrderService.Domain.Events;

public class SagaOrchestratorService : BackgroundService
{
    private readonly IServiceBusPublisher _bus;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<SagaOrchestratorService> _logger;


    public SagaOrchestratorService(IServiceBusPublisher bus, ILogger<SagaOrchestratorService> logger, ServiceBusClient serviceBusClient)
    {
        _bus = bus;
        _logger = logger;
        _serviceBusClient = serviceBusClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Saga Orchestrator Service is starting.");

        var processor = _serviceBusClient.CreateProcessor("OrderTopic", new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += async args =>
        {
            var json = args.Message.Body.ToString();
            var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

            if (orderCreated != null)
            {
                var reserveCommand = new ReserveInventoryCommand
                {
                    OrderId = orderCreated.OrderId,
                    Items = orderCreated.Items
                };

                await _publisher.PublishAsync("InventoryTopic", reserveCommand);
            }

            await args.CompleteMessageAsync(args.Message);
        };

        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine($"Error: {args.Exception.Message}");
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);

        _logger.LogInformation("Saga Orchestrator Service is stopping.");
    }
}
