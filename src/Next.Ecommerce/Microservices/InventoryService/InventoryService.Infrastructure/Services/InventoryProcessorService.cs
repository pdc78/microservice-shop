using System.Text.Json;
using Azure.Messaging.ServiceBus;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using InventoryService.Domain.Events;

namespace InventoryService.Infrastructure.Services;

public class InventoryProcessorService : IInventoryService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<InventoryProcessorService> _logger;

    public InventoryProcessorService(ServiceBusClient serviceBusClient, ILogger<InventoryProcessorService> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }

    public async Task<bool> ReserveInventoryAsync(InventoryReserveEvent evt)
    {
        _logger.LogInformation("Checking inventory for Order {OrderId}", evt.OrderId);

        // Simulate inventory check logic
        var allInStock = evt.Items.All(item => item.Quantity <= 10); // e.g., mock stock level = 10

        return allInStock;
    }

    public async Task SendInventoryConfirmedAsync(InventoryReservedConfirmedEvent evt)
    {
        _logger.LogInformation("Inventory confirmed for Order {OrderId}", evt.OrderId);

        var sender = _serviceBusClient.CreateSender("OrderTopic");
        var message = new ServiceBusMessage(JsonSerializer.Serialize(evt))
        {
            Subject = nameof(InventoryReservedConfirmedEvent)
        };

        await sender.SendMessageAsync(message);
    }

    public async Task SendInventoryRejectedAsync(InventoryReservationFailedEvent evt)
    {
        _logger.LogInformation("Inventory rejected for Order {OrderId}", evt.OrderId);

        var sender = _serviceBusClient.CreateSender("OrderTopic");
        var message = new ServiceBusMessage(JsonSerializer.Serialize(evt))
        {
            Subject = nameof(InventoryReservationFailedEvent)
        };

        await sender.SendMessageAsync(message);
    }

}