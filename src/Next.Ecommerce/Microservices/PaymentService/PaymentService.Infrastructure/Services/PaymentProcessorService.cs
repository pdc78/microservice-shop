using System.Text.Json;
using Azure.Messaging.ServiceBus;
using PaymentService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Contracts.Events;

namespace PaymentService.Infrastructure.Services;

public class PaymentProcessorService : IPaymentService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _serviceBusSender;
    private readonly ILogger<PaymentProcessorService> _logger;

    public PaymentProcessorService(ServiceBusClient serviceBusClient, string topicName, ILogger<PaymentProcessorService> logger)
    {
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient), "ServiceBusClient cannot be null");
        _serviceBusSender = string.IsNullOrEmpty(topicName) ? throw new ArgumentNullException(nameof(topicName), "topicName cannot be null") : _serviceBusClient.CreateSender(topicName);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
    }

    public async Task SendPaymentConfirmedAsync(PaymentConfirmedEvent evt)
    {
        await SendPaymentEventAsync(evt, nameof(PaymentConfirmedEvent), evt.OrderId);
    }

    public async Task SendPaymentRejectedAsync(PaymentFailedEvent evt)
    {
        await SendPaymentEventAsync(evt, nameof(PaymentFailedEvent), evt.OrderId);
    }

    public bool CancelPayment(PaymentCancelledEvent evt)
    {
        _logger.LogInformation("Cancel Payment for Order {OrderId}", evt.OrderId);
        return true;
    }

    private async Task SendPaymentEventAsync<T>(T evt, string messageType, Guid orderId)
    {
        var json = JsonSerializer.Serialize(evt);
        _logger.LogInformation("SendPaymentEventAsync {messageType} for Order {json}", messageType, json);

        var sbMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            ApplicationProperties = { ["messageType"] = messageType, ["orderId"] = orderId.ToString() }
        };

        await _serviceBusSender.SendMessageAsync(sbMessage);
    }
}
