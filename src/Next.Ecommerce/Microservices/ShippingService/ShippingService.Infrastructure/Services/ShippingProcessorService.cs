using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ShippingService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using ShippingService.Domain.Events;

namespace ShippingService.Infrastructure.Services;

public class ShippingProcessorService : IShippingService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<ShippingProcessorService> _logger;

    public ShippingProcessorService(ServiceBusClient serviceBusClient, ILogger<ShippingProcessorService> logger)
    {
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient), "ServiceBusClient cannot be null");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
    }

    public async Task SendShippingConfirmedAsync(ShippingConfirmedEvent evt)
    {
        await SendShippingEventAsync(evt, nameof(ShippingConfirmedEvent), evt.OrderId);
    }

    public async Task SendShippingRejectedAsync(ShippingFailedEvent evt)
    {
        await SendShippingEventAsync(evt, nameof(ShippingFailedEvent), evt.OrderId);
    }

    public bool CancelShipping(ShippingCancelledEvent evt)
    {
        _logger.LogInformation("Cancel Shipping for Order {OrderId}", evt.OrderId);
        return true;
    }

    private async Task SendShippingEventAsync<T>(T evt, string messageType, Guid orderId)
    {
        var json = JsonSerializer.Serialize(evt);
        _logger.LogInformation("SendShippingEventAsync {messageType} for Order {json}", messageType, json);

        var sender = _serviceBusClient.CreateSender("shippingtopic");

        var sbMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            ApplicationProperties = { ["messageType"] = messageType, ["orderId"] = orderId.ToString() }
        };

        await sender.SendMessageAsync(sbMessage);
    }
}
