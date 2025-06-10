using OrderService.Application.Interfaces;
using OrderService.Domain.Events;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using OrderService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;


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

        _logger.LogInformation($"{nameof(SagaOrchestratorService)} - HandleOrderCreatedEvent got a new message");

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

            _logger.LogInformation("Published InventoryRequestedEvent, PaymentRequestedEvent, ShippingRequestedEvent for OrderId {OrderId} to the relatives topics", inventoryEvent.OrderId);
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private async Task EvaluateSagaAsync(SagaState saga, IServiceScope scope)
    {
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var bus = scope.ServiceProvider.GetRequiredService<IServiceBusPublisher>();

        _logger.LogInformation("Evaluating saga state for OrderId {OrderId} | Completed {IsCompleted}", saga.OrderId, saga.IsCompleted);

        if (!saga.IsCompleted)
        {
            _logger.LogInformation("Saga state for OrderId {OrderId} is not completed yet. Current state: InventorySuccess={InventorySuccess}, PaymentSuccess={PaymentSuccess}, ShippingSuccess={ShippingSuccess}", saga.OrderId, saga.InventorySuccess, saga.PaymentSuccess, saga.ShippingSuccess);
            return;
        }

        if (saga.HasAnyFailure)
        {
            var reason = $"Order cancelled due to InventoryFailed={saga.InventoryFailed}, PaymentFailed={saga.PaymentFailed}, ShippingFailed={saga.ShippingFailed}";
            var oId = saga.OrderId.ToString();
            _logger.LogWarning("Saga failed for OrderId {OrderId}, triggering compensation", saga.OrderId);

            if (saga.InventorySuccess)
            {
                await bus.PublishAsync("inventorytopic", oId, nameof(InventoryCancelledEvent), new InventoryCancelledEvent
                {
                    OrderId = saga.OrderId,
                    Items = saga.Items,
                    Reason = reason
                });
                _logger.LogInformation($"Published {nameof(InventoryCancelledEvent)} for OrderId {saga.OrderId}");
            }

            if (saga.PaymentSuccess)
            {
                await bus.PublishAsync("paymenttopic", oId, nameof(PaymentCancelledEvent), new PaymentCancelledEvent
                {
                    OrderId = saga.OrderId,
                    Reason = reason
                });
                _logger.LogInformation($"Published {nameof(PaymentCancelledEvent)} for OrderId {saga.OrderId}");
            }

            if (saga.ShippingSuccess)
            {
                await bus.PublishAsync("shippingtopic", oId, nameof(ShippingCancelledEvent), new ShippingCancelledEvent
                {
                    OrderId = saga.OrderId,
                    Reason = reason
                });
                _logger.LogInformation($"Published {nameof(ShippingCancelledEvent)} for OrderId {saga.OrderId}");
            }

            await orderRepository.UpdateAsync(saga.OrderId, OrderStatus.Rejected);
        }
        else
        {
            _logger.LogInformation("Saga completed successfully for OrderId {OrderId}", saga.OrderId);

            await orderRepository.UpdateAsync(saga.OrderId, OrderStatus.Confirmed);

            _logger.LogInformation("OrderId {OrderId} status updated to Confirmed", saga.OrderId);
        }

        _sagaStates.TryRemove(saga.OrderId, out _);
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
        _logger.LogInformation("Saga Orchestrator Service stopped.");
    }

    private bool TryExtractSagaFromMessage(ProcessMessageEventArgs args, out string messageType, out SagaState saga, out (string WarningMessage, string DeadLetterReason, string DeadLetterDescription) errorReason)
    {
        messageType = null;
        saga = null;
        errorReason = default;

        args.Message.ApplicationProperties.TryGetValue("messageType", out var typeObj);
        args.Message.ApplicationProperties.TryGetValue("orderId", out var orderIdObj);

        if (typeObj is not string mt || orderIdObj is not string orderIdStr || !Guid.TryParse(orderIdStr, out var orderId))
        {
            errorReason = ("Invalid message format", "InvalidMessage", "Missing or invalid type/OrderId");
            return false;
        }

        if (!_sagaStates.TryGetValue(orderId, out saga))
        {
            errorReason = ($"Saga state not found for OrderId {orderId}", "SagaNotFound", "Saga state not initialized");
            return false;
        }

        messageType = mt;
        return true;
    }


    private bool TryExtractMessageMetadata(ProcessMessageEventArgs args, out MessageMetadata? metadata)
    {
        metadata = null;

        if (!args.Message.ApplicationProperties.TryGetValue("messageType", out var typeObj) ||
            !args.Message.ApplicationProperties.TryGetValue("orderId", out var orderIdObj) ||
            typeObj is not string messageType ||
            orderIdObj is not string orderIdStr ||
            !Guid.TryParse(orderIdStr, out var orderId))
        {
            return false;
        }

        metadata = new MessageMetadata
        {
            MessageType = messageType,
            OrderId = orderId,
            JsonBody = args.Message.Body.ToString()
        };

        return true;
    }

    private async Task HandleMessageWithSagaStateAsync(
        ProcessMessageEventArgs args,
        Dictionary<string, Action<SagaState>> messageTypeToAction)
    {
        using var scope = _serviceProvider.CreateScope();

        if (!TryExtractMessageMetadata(args, out var metadata))
        {
            await DeadLetterAsync(args, "InvalidMessage", "Missing or invalid type/OrderId");
            return;
        }

        if (!_sagaStates.TryGetValue(metadata.OrderId, out var saga))
        {
            await DeadLetterAsync(args, "SagaNotFound", $"Saga state not found for OrderId {metadata.OrderId}");
            return;
        }

        try
        {
            _logger.LogInformation("Received message {json} of type {messageType}", metadata.JsonBody, metadata.MessageType);

            if (string.IsNullOrWhiteSpace(metadata.JsonBody))
            {
                await DeadLetterAsync(args, "EmptyMessageBody", "Message body is empty or null.");
                return;
            }

            if (!messageTypeToAction.TryGetValue(metadata.MessageType, out var stateUpdateAction))
            {
                await DeadLetterAsync(args, "UnknownType", $"Unrecognized message type: {metadata.MessageType}");
                return;
            }

            stateUpdateAction(saga);

            await EvaluateSagaAsync(saga, scope);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (JsonException ex)
        {
            await DeadLetterAsync(args, "DeserializationFailed", ex.Message, ex);
        }
        catch (Exception ex)
        {
            await DeadLetterAsync(args, "ProcessingError", ex.Message, ex);
        }
    }

    private Task DeadLetterAsync(ProcessMessageEventArgs args, string reason, string description, Exception? ex = null)
    {
        _logger.LogWarning(ex, "Dead-lettering message. Reason: {reason}, Description: {description}", reason, description);
        return args.DeadLetterMessageAsync(args.Message, reason, description);
    }


    // Handlers for each topic

    public Task HandlePaymentEvents(ProcessMessageEventArgs args) =>
        HandleMessageWithSagaStateAsync(args, new()
        {
            { nameof(PaymentConfirmedEvent), saga => saga.PaymentSuccess = true },
            { nameof(PaymentFailedEvent), saga => saga.PaymentFailed = true }
        });

    public Task HandleShippingEvents(ProcessMessageEventArgs args) =>
        HandleMessageWithSagaStateAsync(args, new()
        {
            { nameof(ShippingConfirmedEvent), saga => saga.ShippingSuccess = true },
            { nameof(ShippingFailedEvent), saga => saga.ShippingFailed = true }
        });

    public Task HandleInventoryEvents(ProcessMessageEventArgs args) =>
        HandleMessageWithSagaStateAsync(args, new()
        {
            { nameof(InventoryConfirmedEvent), saga => saga.InventorySuccess = true },
            { nameof(InventoryCheckFailedEvent), saga => saga.InventoryFailed = true }
        });
}