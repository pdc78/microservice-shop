using OrderService.Application.Interfaces;
using OrderService.Domain.Events;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using OrderService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Text;


public class SagaOrchestratorService : BackgroundService
{

    private readonly IServiceProvider _serviceProvider;
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<SagaOrchestratorService> _logger;
    private readonly List<ServiceBusProcessor> _processors = new();


    private static readonly ConcurrentDictionary<Guid, SagaState> _sagaStates = new();


    public SagaOrchestratorService(IServiceProvider serviceProvider, ServiceBusClient serviceBusClient, ILogger<SagaOrchestratorService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(ServiceBusClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<SagaOrchestratorService>));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Saga Orchestrator Service starting.");

        await RegisterProcessorAsync("ordertopic", "order-subscription-all", HandleOrderCreatedEvent, stoppingToken);
        await RegisterProcessorAsync("inventorytopic", "inventory-sub-response", HandleInventoryEvents, stoppingToken);
        await RegisterProcessorAsync("paymenttopic", "payment-sub-response", HandlePaymentEvents, stoppingToken);
        await RegisterProcessorAsync("shippingtopic", "shipping-sub-response", HandleShippingEvents, stoppingToken);

        //    await RegisterProcessorAsync(
        //        "ordertopic",
        //        "order-subscription-all",
        //          args => HandleEventAsync<IIntegrationEvent>(
        //    args,
        //    (evt, orderId) => HandleOrderCreatedAsync(evt, orderId),
        //    stoppingToken
        //),
        //stoppingToken);

        //    await RegisterProcessorAsync(
        //        "inventorytopic",
        //        "inventory-sub-response",
        //        args => HandleEventAsync<IIntegrationEvent>(args, HandleInventorySagaAsync, stoppingToken),
        //        stoppingToken);

        //    await RegisterProcessorAsync(
        //       "paymenttopic",
        //       "payment-sub-response",
        //       args => HandleEventAsync<IIntegrationEvent>(args, HandlePaymentSagaAsync, stoppingToken),
        //       stoppingToken);

        //    //await RegisterProcessorAsync(
        //  "shippingtopic",
        //  "shipping-sub-response",
        //  args => HandleEventAsync<IIntegrationEvent>(args, HandleShippingSagaAsync, stoppingToken),
        //  stoppingToken);

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

            string oId = orderCreated.OrderId.ToString();

            // Initialize saga state
            var sagaState = new SagaState
            {
                OrderId = orderCreated.OrderId
            };
            _sagaStates.TryAdd(orderCreated.OrderId, sagaState);
            _logger.LogInformation("Initialized SagaState for OrderId {OrderId}", orderCreated.OrderId);

            var inventoryEvent = new InventoryRequestedEvent
            {
                OrderId = orderCreated.OrderId,
                Items = orderCreated.Items
            };

            var paymentEvent = new PaymentRequestedEvent
            {
                OrderId = orderCreated.OrderId,
                Amount = orderCreated.TotalAmount
            };

            var shippingEvent = new ShippingRequestedEvent
            {
                OrderId = orderCreated.OrderId,
                Address = orderCreated.ShippingAddress
            };

            await bus.PublishAsync("inventorytopic", oId, nameof(InventoryRequestedEvent), inventoryEvent);
            await bus.PublishAsync("paymenttopic", oId, nameof(PaymentRequestedEvent), paymentEvent);
            await bus.PublishAsync("shippingtopic", oId, nameof(ShippingRequestedEvent), shippingEvent);

            _logger.LogInformation("Published InventoryReserveRequestEvent for OrderId {OrderId} to the topic inventorytopic", inventoryEvent.OrderId);
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private async Task HandleInventoryEvents(ProcessMessageEventArgs args)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            _logger.LogInformation($"{nameof(SagaOrchestratorService)} - HandleInventoryEvents got a new message");
            args.Message.ApplicationProperties.TryGetValue("messageType", out var typeObj);
            args.Message.ApplicationProperties.TryGetValue("orderId", out var orderIdObj);

            if (typeObj is not string messageType || orderIdObj is not string orderIdStr || !Guid.TryParse(orderIdStr, out var orderId))
            {
                _logger.LogWarning("Invalid message format");
                await args.DeadLetterMessageAsync(args.Message, "InvalidMessage", "Missing or invalid type/OrderId");
                return;
            }

            if (!_sagaStates.TryGetValue(orderId, out var saga))
            {
                _logger.LogWarning("Saga state not found for OrderId {OrderId}", orderId);
                await args.DeadLetterMessageAsync(args.Message, "SagaNotFound", "Saga state not initialized");
                return;
            }
            var json = args.Message.Body.ToString();
            _logger.LogInformation("Received message {json} of type {messageType}", json, messageType);
            if (!string.IsNullOrWhiteSpace(json))
            {
                switch (messageType)
                {
                    case nameof(InventoryConfirmedEvent):
                        saga.InventorySuccess = true;
                        break;

                    case nameof(InventoryCheckFailedEvent):
                        saga.InventoryFailed = true;
                        break;

                    default:
                        _logger.LogWarning("Unknown message type: {MessageType}", messageType);
                        await args.DeadLetterMessageAsync(args.Message, "UnknownType", "Unrecognized message type");
                        return;
                }
            }
            else
            {
                _logger.LogWarning("Message body is empty or null.");
                await args.DeadLetterMessageAsync(args.Message, "EmptyMessageBody", "Message body is empty or null.");
            }
            // Complete the message after processing  
            await EvaluateSagaAsync(saga, scope);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message from InventoryTopic.");
            await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", ex.Message);
        }
    }

    private async Task HandleShippingEvents(ProcessMessageEventArgs args)
    {
        using var scope = _serviceProvider.CreateScope();
        try
        {
            _logger.LogInformation($"{nameof(SagaOrchestratorService)} - HandleShippingEvents got a new message");
            args.Message.ApplicationProperties.TryGetValue("messageType", out var typeObj);
            args.Message.ApplicationProperties.TryGetValue("orderId", out var orderIdObj);

            if (typeObj is not string messageType || orderIdObj is not string orderIdStr || !Guid.TryParse(orderIdStr, out var orderId))
            {
                _logger.LogWarning("Invalid message format");
                await args.DeadLetterMessageAsync(args.Message, "InvalidMessage", "Missing or invalid type/OrderId");
                return;
            }

            if (!_sagaStates.TryGetValue(orderId, out var saga))
            {
                _logger.LogWarning("Saga state not found for OrderId {OrderId}", orderId);
                await args.DeadLetterMessageAsync(args.Message, "SagaNotFound", "Saga state not initialized");
                return;
            }

            var json = args.Message.Body.ToString();
            _logger.LogInformation("Received message {json} of type {messageType}", json, messageType);

            if (!string.IsNullOrWhiteSpace(json))
            {
                switch (messageType)
                {
                    case nameof(ShippingConfirmedEvent):
                        saga.InventorySuccess = true;
                        break;

                    case nameof(ShippingFailedEvent):
                        saga.InventoryFailed = true;
                        break;

                    default:
                        _logger.LogWarning("Unknown message type: {MessageType}", messageType);
                        await args.DeadLetterMessageAsync(args.Message, "UnknownType", "Unrecognized message type");
                        return;
                }
            }
            else
            {
                _logger.LogWarning("Message body is empty or null.");
                await args.DeadLetterMessageAsync(args.Message, "EmptyMessageBody", "Message body is empty or null.");
            }

            // Complete the message after processing  
            await EvaluateSagaAsync(saga, scope);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message from InventoryTopic.");
            await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing shipping events.");
            await args.DeadLetterMessageAsync(args.Message, "ProcessingError", ex.Message);
        }
    }

    private async Task HandlePaymentEvents(ProcessMessageEventArgs args)
    {
        using var scope = _serviceProvider.CreateScope();
        try
        {
            _logger.LogInformation($"{nameof(SagaOrchestratorService)} - HandlePaymentEvents got a new message");
            args.Message.ApplicationProperties.TryGetValue("messageType", out var typeObj);
            args.Message.ApplicationProperties.TryGetValue("orderId", out var orderIdObj);

            if (typeObj is not string messageType || orderIdObj is not string orderIdStr || !Guid.TryParse(orderIdStr, out var orderId))
            {
                _logger.LogWarning("Invalid message format");
                await args.DeadLetterMessageAsync(args.Message, "InvalidMessage", "Missing or invalid type/OrderId");
                return;
            }

            if (!_sagaStates.TryGetValue(orderId, out var saga))
            {
                _logger.LogWarning("Saga state not found for OrderId {OrderId}", orderId);
                await args.DeadLetterMessageAsync(args.Message, "SagaNotFound", "Saga state not initialized");
                return;
            }

            var json = args.Message.Body.ToString();
            _logger.LogInformation("Received message {json} of type {messageType}", json, messageType);

            if (!string.IsNullOrWhiteSpace(json))
            {
                switch (messageType)
                {
                    case nameof(PaymentConfirmedEvent):
                        saga.InventorySuccess = true;
                        break;

                    case nameof(PaymentFailedEvent):
                        saga.InventoryFailed = true;
                        break;

                    default:
                        _logger.LogWarning("Unknown message type: {MessageType}", messageType);
                        await args.DeadLetterMessageAsync(args.Message, "UnknownType", "Unrecognized message type");
                        return;
                }
            }
            else
            {
                _logger.LogWarning("Message body is empty or null.");
                await args.DeadLetterMessageAsync(args.Message, "EmptyMessageBody", "Message body is empty or null.");
            }

            // Complete the message after processing  
            await EvaluateSagaAsync(saga, scope);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message from InventoryTopic.");
            await args.DeadLetterMessageAsync(args.Message, "DeserializationFailed", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing shipping events.");
            await args.DeadLetterMessageAsync(args.Message, "ProcessingError", ex.Message);
        }
    }


    private async Task EvaluateSagaAsync(SagaState saga, IServiceScope scope)
    {
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var bus = scope.ServiceProvider.GetRequiredService<IServiceBusPublisher>();

        _logger.LogInformation("Evaluating saga state for OrderId {OrderId}", saga.OrderId);

        if (!saga.IsCompleted)
        {
            _logger.BeginScope("Saga state for OrderId {OrderId} is not completed yet. Current state: InventorySuccess={InventorySuccess}, PaymentSuccess={PaymentSuccess}, ShippingSuccess={ShippingSuccess}, InventoryFailed={InventoryFailed}, PaymentFailed={PaymentFailed}, ShippingFailed={ShippingFailed}", saga.OrderId, saga.InventorySuccess, saga.PaymentSuccess, saga.ShippingSuccess, saga.InventoryFailed, saga.PaymentFailed, saga.ShippingFailed);
            return;
        }

        if (saga.HasAnyFailure)
        {
            var oId = saga.OrderId.ToString();
            _logger.LogWarning("Saga failed for OrderId {OrderId}, triggering compensation", saga.OrderId);

            if (saga.InventorySuccess)
            {
                await bus.PublishAsync("inventorytopic", oId, nameof(InventoryCancelledEvent), new InventoryCancelledEvent
                {
                    OrderId = saga.OrderId,
                    Items = saga.Items
                });
                _logger.LogInformation($"Published {nameof(InventoryCancelledEvent)} for OrderId {saga.OrderId}");
            }

            if (saga.PaymentSuccess)
            {
                await bus.PublishAsync("paymenttopic", oId, nameof(PaymentCancelledEvent), new PaymentCancelledEvent
                {
                    OrderId = saga.OrderId,
                    Reason = "Order cancelled due to inventory failure"
                });
                _logger.LogInformation($"Published {nameof(PaymentCancelledEvent)} for OrderId {saga.OrderId}");
            }

            if (saga.ShippingSuccess)
            {
                await bus.PublishAsync("shippingtopic", oId, nameof(ShippingCancelledEvent), new ShippingCancelledEvent
                {
                    OrderId = saga.OrderId,
                    Reason = "Order cancelled due to inventory failure"
                });
                _logger.LogInformation($"Published {nameof(ShippingCancelledEvent)} for OrderId {saga.OrderId}");
            }

            await orderRepository.UpdateAsync(saga.OrderId, OrderStatus.Rejected);
        }
        else
        {
            _logger.LogInformation("Saga completed successfully for OrderId {OrderId}", saga.OrderId);
            await orderRepository.UpdateAsync(saga.OrderId, OrderStatus.Confirmed);
        }

        _sagaStates.TryRemove(saga.OrderId, out _);
    }

    //private async Task HandleEventAsync<TEvent>(
    //    ProcessMessageEventArgs args,
    //    Func<IIntegrationEvent, Guid, Task> handleFunc,
    //    CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        if (!args.Message.ApplicationProperties.TryGetValue("messageType", out var typeObj) ||
    //            !args.Message.ApplicationProperties.TryGetValue("orderId", out var orderIdObj) ||
    //            typeObj is not string messageType ||
    //            orderIdObj is not string orderIdStr ||
    //            !Guid.TryParse(orderIdStr, out var orderId))
    //        {
    //            _logger.LogWarning("Invalid metadata in message");
    //            await args.DeadLetterMessageAsync(args.Message, "InvalidMetadata", "Missing or invalid type/OrderId");
    //            return;
    //        }

    //        var json = Encoding.UTF8.GetString(args.Message.Body);
    //        //var evt = JsonSerializer.Deserialize<TEvent>(json);
    //        IIntegrationEvent evt = DeserializeEvent(json, messageType);

    //        if (evt is null)
    //        {
    //            _logger.LogWarning("Deserialization returned null");
    //            await args.DeadLetterMessageAsync(args.Message, "NullDeserialization", "Deserialized event is null");
    //            return;
    //        }

    //        _logger.LogInformation("Handling event of type {EventType} with OrderId {OrderId}", typeof(TEvent).Name, orderId);

    //        await handleFunc(evt, orderId);

    //        await args.CompleteMessageAsync(args.Message);
    //    }
    //    catch (JsonException ex)
    //    {
    //        _logger.LogError(ex, "Deserialization failed for {EventType}", typeof(TEvent).Name);
    //        await args.DeadLetterMessageAsync(args.Message, "DeserializationError", ex.Message);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Unexpected error while handling event {EventType}", typeof(TEvent).Name);
    //        // Decide whether to complete, abandon, or let retry
    //    }
    //}

    //private Task HandleInventorySagaAsync(IIntegrationEvent evt, Guid orderId)
    //{
    //    using var scope = _serviceProvider.CreateScope();
    //    if (!_sagaStates.TryGetValue(orderId, out var saga))
    //    {
    //        _logger.LogWarning("No saga found for order {OrderId}", orderId);
    //        return Task.CompletedTask;
    //    }

    //    switch (evt)
    //    {
    //        case InventoryConfirmedEvent:
    //            saga.InventorySuccess = true;
    //            break;
    //        case InventoryCheckFailedEvent:
    //            saga.InventoryFailed = true;
    //            break;
    //    }

    //    return EvaluateSagaAsync(saga, scope);
    //}

    //private Task HandleShippingSagaAsync(IIntegrationEvent evt, Guid orderId)
    //{
    //    using var scope = _serviceProvider.CreateScope();
    //    if (!_sagaStates.TryGetValue(orderId, out var saga))
    //    {
    //        _logger.LogWarning("No saga found for order {OrderId}", orderId);
    //        return Task.CompletedTask;
    //    }

    //    switch (evt)
    //    {
    //        case PaymentConfirmedEvent:
    //            saga.InventorySuccess = true;
    //            break;
    //        case PaymentFailedEvent:
    //            saga.InventoryFailed = true;
    //            break;
    //    }

    //    return EvaluateSagaAsync(saga, scope);
    //}

    //private Task HandlePaymentSagaAsync(IIntegrationEvent evt, Guid orderId)
    //{
    //    using var scope = _serviceProvider.CreateScope();
    //    if (!_sagaStates.TryGetValue(orderId, out var saga))
    //    {
    //        _logger.LogWarning("No saga found for order {OrderId}", orderId);
    //        return Task.CompletedTask;
    //    }

    //    switch (evt)
    //    {
    //        case PaymentConfirmedEvent:
    //            saga.InventorySuccess = true;
    //            break;
    //        case PaymentFailedEvent:
    //            saga.InventoryFailed = true;
    //            break;
    //    }

    //    return EvaluateSagaAsync(saga, scope);
    //}

    //private async Task HandleOrderCreatedAsync(OrderCreatedEvent orderCreated, Guid orderId)
    //{
    //    using var scope = _serviceProvider.CreateScope();
    //    var bus = scope.ServiceProvider.GetRequiredService<IServiceBusPublisher>();
    //    var oId = orderId.ToString();

    //    _logger.LogInformation("Received OrderCreatedEvent for OrderId {OrderId}", orderId);

    //    // Initialize saga state
    //    var sagaState = new SagaState
    //    {
    //        OrderId = orderId
    //    };
    //    _sagaStates.TryAdd(orderId, sagaState);
    //    _logger.LogInformation("Initialized SagaState for OrderId {OrderId}", orderId);

    //    var inventoryEvent = new InventoryRequestedEvent
    //    {
    //        OrderId = orderId,
    //        Items = orderCreated.Items
    //    };

    //    var paymentEvent = new PaymentRequestedEvent
    //    {
    //        OrderId = orderId,
    //        Amount = orderCreated.TotalAmount
    //    };

    //    var shippingEvent = new ShippingRequestedEvent
    //    {
    //        OrderId = orderId,
    //        Address = orderCreated.ShippingAddress
    //    };

    //    await bus.PublishAsync("inventorytopic", oId, nameof(InventoryRequestedEvent), inventoryEvent);
    //    await bus.PublishAsync("paymenttopic", oId, nameof(PaymentRequestedEvent), paymentEvent);
    //    await bus.PublishAsync("shippingtopic", oId, nameof(ShippingRequestedEvent), shippingEvent);

    //    _logger.LogInformation("Published all requested events for OrderId {OrderId}", orderId);
    //}

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
        _logger.LogInformation("Saga Orchestrator Service stopped.");
    }

    //private IIntegrationEvent DeserializeEvent(string json, string messageType)
    //{
    //    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    //    return messageType switch
    //    {
    //        "OrderCreatedEvent" => JsonSerializer.Deserialize<OrderCreatedEvent>(json, options),
    //        "InventoryCheckFailedEvent" => JsonSerializer.Deserialize<InventoryCheckFailedEvent>(json, options),
    //        "InventoryConfirmedEvent" => JsonSerializer.Deserialize<InventoryConfirmedEvent>(json, options),
    //        _ => throw new NotSupportedException($"Unknown message type: {messageType}")
    //    };
    //}
}
