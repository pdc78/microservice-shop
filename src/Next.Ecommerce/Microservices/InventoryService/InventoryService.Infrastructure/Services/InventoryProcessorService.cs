using System.Text.Json;
using Azure.Messaging.ServiceBus;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using InventoryService.Domain.Events;
using System.Diagnostics.CodeAnalysis;

namespace InventoryService.Infrastructure.Services;

public class InventoryProcessorService : IInventoryService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<InventoryProcessorService> _logger;

    public InventoryProcessorService(ServiceBusClient serviceBusClient, ILogger<InventoryProcessorService> logger)
    {
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient), "ServiceBusClient cannot be null");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null");
    }

    public bool ReserveInventory(InventoryReserveRequestEvent evt)
    {
        // Simulate inventory check logic
        var tot = evt.Items.Sum(item => item.Quantity);
        bool allInStock = tot <= 10; // e.g., mock stock level = 10

        _logger.LogInformation("Checking inventory for Order {OrderId} Tot {tot} allInStock : {allInStock}", evt.OrderId, tot, allInStock);

        return allInStock;
    }

    public async Task SendInventoryConfirmedAsync(InventoryReservedConfirmedEvent evt)
    {
        await SendInventoryEventAsync(evt, nameof(InventoryReservedConfirmedEvent));
    }

    public async Task SendInventoryRejectedAsync(InventoryReservationFailedEvent evt)
    {
        await SendInventoryEventAsync(evt, nameof(InventoryReservationFailedEvent));
    }

    private async Task SendInventoryEventAsync<T>(T evt, string messageType)
    {
        var json = JsonSerializer.Serialize(evt);
        _logger.LogInformation("Inventory {MessageType} for Order {Payload}", messageType, json);

        var sender = _serviceBusClient.CreateSender("OrderTopic");

        var serviceBusMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json"
        };
        serviceBusMessage.ApplicationProperties["messageType"] = messageType;

        await sender.SendMessageAsync(serviceBusMessage);
    }
}
