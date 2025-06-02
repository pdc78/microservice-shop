using OrderService.Application.Interfaces;
using OrderService.Domain.Events;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using OrderService.Domain.Entities;

public class SagaOrchestratorService : BackgroundService
{
    private readonly IServiceBusPublisher _bus;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<SagaOrchestratorService> _logger;
    private readonly IOrderRepository _orderRepository;

    private readonly List<ServiceBusProcessor> _processors = new();


    public SagaOrchestratorService(IServiceBusPublisher bus, ILogger<SagaOrchestratorService> logger, ServiceBusClient serviceBusClient, IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
        _bus = bus;
        _logger = logger;
        _serviceBusClient = serviceBusClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Saga Orchestrator Service started.");

        await RegisterProcessorAsync("OrderTopic", HandleOrderCreatedEvent, stoppingToken);
        await RegisterProcessorAsync("InventoryTopic", HandleInventoryEvents, stoppingToken);

        // In the future, easily add:
        // await RegisterProcessorAsync("PaymentTopic", HandlePaymentEvents, stoppingToken);
        // await RegisterProcessorAsync("ShippingTopic", HandleShippingEvents, stoppingToken);

        _logger.LogInformation("Saga Orchestrator Service started.");
    }



    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     _logger.LogInformation("Saga Orchestrator Service is starting.");

    //     var orderProcessor = _serviceBusClient.CreateProcessor("OrderTopic", new ServiceBusProcessorOptions
    //     {
    //         MaxConcurrentCalls = 1,
    //         AutoCompleteMessages = false
    //     });

    //     orderProcessor.ProcessMessageAsync += async args =>
    //     {
    //         try
    //         {
    //             var json = args.Message.Body.ToString();
    //             var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

    //             if (orderCreated != null)
    //             {
    //                 var reserveCommand = new ReserveInventoryEvent
    //                 {
    //                     OrderId = orderCreated.OrderId,
    //                     Items = orderCreated.Items
    //                 };

    //                 await _bus.PublishAsync("InventoryTopic", reserveCommand);
    //                 _logger.LogInformation("ReserveInventoryEvent published. OrderId: {OrderId}", reserveCommand.OrderId);
    //             }

    //             await args.CompleteMessageAsync(args.Message);
    //         }
    //         catch (Exception ex)
    //         {
    //             _logger.LogError(ex, "Error processing OrderCreatedEvent");
    //             // Optionally abandon message for retry depending on policy
    //             await args.AbandonMessageAsync(args.Message);
    //         }
    //     };

    //     orderProcessor.ProcessErrorAsync += args =>
    //     {
    //         Console.WriteLine($"Error: {args.Exception.Message}");
    //         return Task.CompletedTask;
    //     };

    //     await orderProcessor.StartProcessingAsync(stoppingToken);
    //     _logger.LogInformation("Saga Orchestrator Service is stopping.");

    //     // Handle InventoryReservationFailedEvent from InventoryTopic
    //     var inventoryProcessor = _serviceBusClient.CreateProcessor("InventoryTopic", new ServiceBusProcessorOptions());

    //     inventoryProcessor.ProcessMessageAsync += async args =>
    //     {
    //         var json = args.Message.Body.ToString();
    //         var failedEvent = JsonSerializer.Deserialize<InventoryReservationFailedEvent>(json);

    //         if (failedEvent != null)
    //         {
    //             _logger.LogWarning("InventoryReservationFailedEvent received for OrderId: {OrderId}. Reason: {Reason}",
    //                 failedEvent.OrderId, failedEvent.Reason);

    //             // Compensating action: update order to Rejected
    //             // await _repository.UpdateStatusAsync(failedEvent.OrderId, OrderStatus.Rejected);
    //             _logger.LogInformation("Order {OrderId} marked as Rejected - call the repository to update the order", failedEvent.OrderId);
    //         }

    //         await args.CompleteMessageAsync(args.Message);
    //     };

    //     inventoryProcessor.ProcessErrorAsync += args =>
    //     {
    //         _logger.LogError(args.Exception, "Error processing InventoryTopic message");
    //         return Task.CompletedTask;
    //     };

    //     await inventoryProcessor.StartProcessingAsync(stoppingToken);
    //     _logger.LogInformation("Saga Orchestrator Service started successfully.");

    // }



    private async Task RegisterProcessorAsync(string topic, Func<ProcessMessageEventArgs, Task> handler, CancellationToken token)
    {
        var processor = _serviceBusClient.CreateProcessor(topic, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += handler;
        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Error processing message from {Topic}.", topic);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(token);
        _processors.Add(processor);

        _logger.LogInformation("Started listening to {Topic}", topic);
    }

    private async Task HandleOrderCreatedEvent(ProcessMessageEventArgs args)
    {
        var json = args.Message.Body.ToString();
        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

        if (orderCreated != null)
        {
            _logger.LogInformation("Received OrderCreatedEvent for OrderId {OrderId}", orderCreated.OrderId);

            var reserveEvent = new InventoryReserveEvent
            {
                OrderId = orderCreated.OrderId,
                Items = orderCreated.Items
            };

            await _bus.PublishAsync("InventoryTopic", reserveEvent);
            _logger.LogInformation("Published ReserveInventoryEvent for OrderId {OrderId}", reserveEvent.OrderId);
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private async Task HandleInventoryEvents(ProcessMessageEventArgs args)
    {
        var json = args.Message.Body.ToString();

        try
        {
            var failedEvent = JsonSerializer.Deserialize<InventoryReservationFailedEvent>(json);

            if (json.Contains(nameof(InventoryReservedConfirmedEvent)))
            {
                var reserved = JsonSerializer.Deserialize<InventoryReservedConfirmedEvent>(json);
                if (reserved != null)
                {
                    await _orderRepository.UpdateAsync(reserved.OrderId, OrderStatus.Confirmed);
                    _logger.LogInformation("Order {OrderId} marked as Confirmed", reserved.OrderId);
                }
            }
            else if (json.Contains(nameof(InventoryReservationFailedEvent)))
            {
                var rejected = JsonSerializer.Deserialize<InventoryReservationFailedEvent>(json);
                if (rejected != null)
                {
                    await _orderRepository.UpdateAsync(rejected.OrderId, OrderStatus.Rejected);
                    _logger.LogInformation("Order {OrderId} marked as Rejected", rejected.OrderId);

                }
            }
            await args.CompleteMessageAsync(args.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message from InventoryTopic.");
            await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saga Orchestrator Service is stopping.");
        await base.StopAsync(cancellationToken);
    }
}
