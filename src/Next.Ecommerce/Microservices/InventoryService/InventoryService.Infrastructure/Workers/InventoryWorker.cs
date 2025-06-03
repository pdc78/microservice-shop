using Azure.Messaging.ServiceBus;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InventoryService.Infrastructure.Workers
{
    public class InventoryWorker : BackgroundService
    {
        private readonly ILogger<InventoryWorker> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IInventoryService _inventoryService;

        public InventoryWorker(
            ILogger<InventoryWorker> logger,
            ServiceBusClient serviceBusClient,
            IInventoryService inventoryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Inventory Worker started");

            var processor = _serviceBusClient.CreateProcessor("inventorytopic", "inventory-subscription-all", new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var jsonObj = args.Message.Body.ToString();
                    var reserveRequest = JsonSerializer.Deserialize<InventoryReserveRequestEvent>(jsonObj);

                    if (reserveRequest != null)
                    {
                        _logger.LogInformation("Received InventoryReserveRequestEvent for OrderId {OrderId} json reserveRequest: {jsonObj}", reserveRequest.OrderId, jsonObj);
                        var success = _inventoryService.ReserveInventory(reserveRequest);

                        if (success)
                        {
                            _logger.LogInformation("Published InventoryReservedConfirmedEvent for OrderId {OrderId}", reserveRequest.OrderId);

                            var confirmed = new InventoryReservedConfirmedEvent
                            {
                                OrderId = reserveRequest.OrderId
                            };

                            await _inventoryService.SendInventoryConfirmedAsync(confirmed);
                        }
                        else
                        {
                            _logger.LogInformation("Published InventoryReservationFailedEvent for OrderId {OrderId}", reserveRequest.OrderId);

                            var rejected = new InventoryReservationFailedEvent
                            {
                                OrderId = reserveRequest.OrderId,
                                Reason = "Not enough stock"
                            };

                            await _inventoryService.SendInventoryRejectedAsync(rejected);
                        }
                    }

                    await args.CompleteMessageAsync(args.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{nameof(InventoryWorker)} : Error processing inventory message");
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, $"{nameof(InventoryWorker)} Service Bus error");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);
        }
    }
}
