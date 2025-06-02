using OrderService.Application.Interfaces;
using OrderService.Domain.Events;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using OrderService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Amqp.Framing;

public class SagaOrchestratorService : BackgroundService
{

    private readonly IServiceProvider _serviceProvider;
    // private readonly IServiceBusPublisher _bus;
    private readonly ServiceBusClient _serviceBusClient;
    //private readonly IOrderRepository _orderRepository;
    private readonly ILogger<SagaOrchestratorService> _logger;

    private readonly List<ServiceBusProcessor> _processors = new();

    //  IOrderRepository orderRepository
    public SagaOrchestratorService(IServiceProvider serviceProvider, ServiceBusClient serviceBusClient, ILogger<SagaOrchestratorService> logger)
    {

        //_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(IServiceBusPublisher));
        //  _bus = bus ?? throw new ArgumentNullException(nameof(IServiceBusPublisher));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(ServiceBusClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<SagaOrchestratorService>));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Saga Orchestrator Service starting.");

        await RegisterProcessorAsync("ordertopic", "order-subscription-all", HandleOrderCreatedEvent, stoppingToken);
        await RegisterProcessorAsync("inventorytopic", "inventory-subscription-all", HandleInventoryEvents, stoppingToken);

        // In the future, easily add:
        // await RegisterProcessorAsync("PaymentTopic", HandlePaymentEvents, stoppingToken);
        // await RegisterProcessorAsync("ShippingTopic", HandleShippingEvents, stoppingToken);

        _logger.LogInformation("Saga Orchestrator Service started.");
    }

    private async Task RegisterProcessorAsync(string topic, string subscription, Func<ProcessMessageEventArgs, Task> handler, CancellationToken token)
    {
        var processor = _serviceBusClient.CreateProcessor(topic, subscription, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += handler;
        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Error processing message from {Topic}.", topic);
            return Task.CompletedTask;
        };

        try
        {
            await processor.StartProcessingAsync(token);
            _processors.Add(processor);
            _logger.LogInformation("Started listening to {Topic}", topic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start processor for topic {Topic}", topic);
            throw;
        }
    }

    private async Task HandleOrderCreatedEvent(ProcessMessageEventArgs args)
    {
        using var scope = _serviceProvider.CreateScope();
        var bus = scope.ServiceProvider.GetRequiredService<IServiceBusPublisher>();

        var json = args.Message.Body.ToString();
        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(json);

        if (orderCreated != null)
        {
            _logger.LogInformation("Received OrderCreatedEvent for OrderId {OrderId}", orderCreated.OrderId);

            var reserveEvent = new InventoryReserveRequestEvent
            {
                OrderId = orderCreated.OrderId,
                Items = orderCreated.Items
            };

            await bus.PublishAsync("inventorytopic", reserveEvent);
            _logger.LogInformation("Published InventoryReserveRequestEvent for OrderId {OrderId}", reserveEvent.OrderId);
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private async Task HandleInventoryEvents(ProcessMessageEventArgs args)
    {
        using var scope = _serviceProvider.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

        var json = args.Message.Body.ToString();

        try
        {
            _logger.LogInformation($"HandleInventoryEvents started : {args.Message.Body.ToString()}");

            if (json.Contains(nameof(InventoryReservedConfirmedEvent)))
            {
                _logger.LogInformation("Received InventoryReservedConfirmedEvent");
                var reserved = JsonSerializer.Deserialize<InventoryReservedConfirmedEvent>(json);
                if (reserved != null)
                {
                    await orderRepository.UpdateAsync(reserved.OrderId, OrderStatus.Confirmed);
                    _logger.LogInformation("Order {OrderId} marked as Confirmed", reserved.OrderId);
                }
            }
            else if (json.Contains(nameof(InventoryReservationFailedEvent)))
            {
                _logger.LogInformation("Received InventoryReservationFailedEvent");
                var rejected = JsonSerializer.Deserialize<InventoryReservationFailedEvent>(json);
                if (rejected != null)
                {
                    await orderRepository.UpdateAsync(rejected.OrderId, OrderStatus.Rejected);
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

        foreach (var processor in _processors)
        {
            try
            {
                await processor.StopProcessingAsync(cancellationToken);
                await processor.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while stopping/disposing processor.");
            }
        }

        _processors.Clear();

        await base.StopAsync(cancellationToken);
    }

}
