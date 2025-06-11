using System.Text.Json;
using Azure.Messaging.ServiceBus;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Contracts.Events;

namespace InventoryService.Infrastructure.Services;

public class InventoryProcessorService : IInventoryService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _serviceBusSender;
    private readonly ILogger<InventoryProcessorService> _logger;

    public InventoryProcessorService(ServiceBusClient serviceBusClient, string topicName, ILogger<InventoryProcessorService> logger)
    {
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient), "ServiceBusClient cannot be null");
        _serviceBusSender = string.IsNullOrEmpty(topicName) ? throw new ArgumentNullException(nameof(topicName), "topicName cannot be null") : _serviceBusClient.CreateSender(topicName);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
    }

    public bool ReserveInventory(InventoryRequestedEvent evt)
    {
        // Simulate inventory check logic
        var tot = evt.Items.Sum(item => item.Quantity);
        bool allInStock = tot <= 10; // e.g., mock stock level = 10

        _logger.LogInformation("Checking inventory for Order {OrderId} Tot {tot} allInStock : {allInStock}", evt.OrderId, tot, allInStock);

        return allInStock;
    }

    public async Task SendInventoryConfirmedAsync(InventoryConfirmedEvent evt)
    {
        await SendInventoryEventAsync(evt, nameof(InventoryConfirmedEvent), evt.OrderId);
    }

    public async Task SendInventoryRejectedAsync(InventoryCheckFailedEvent evt)
    {
        await SendInventoryEventAsync(evt, nameof(InventoryCheckFailedEvent), evt.OrderId);
    }

    public bool UnreserveInventory(InventoryCancelledEvent evt)
    {
        _logger.LogInformation("Unreserving inventory for Order {OrderId}", evt.OrderId);
        return true; // Simulate unreservation logic
    }

    private async Task SendInventoryEventAsync<T>(T evt, string messageType, Guid orderId)
    {
        var json = JsonSerializer.Serialize(evt);
        _logger.LogInformation("SendInventoryEventAsync {messageType} for Order {json}", messageType, json);

        var sbMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            ApplicationProperties = { ["messageType"] = messageType, ["orderId"] = orderId.ToString() }
        };

        await _serviceBusSender.SendMessageAsync(sbMessage);
    }
}
