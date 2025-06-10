using Azure.Messaging.ServiceBus;
using InventoryService.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Contracts.Events;
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

            var processor = _serviceBusClient.CreateProcessor("inventorytopic", "inventory-sub-request", new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var messageType = args.Message.ApplicationProperties.TryGetValue("messageType", out var type) ? type.ToString() : string.Empty;
                    _logger.LogInformation($"Received message of type {messageType} possible values {nameof(InventoryRequestedEvent)} - {nameof(InventoryCancelledEvent)}");
                    if (messageType != nameof(InventoryRequestedEvent) && messageType != nameof(InventoryCancelledEvent))
                    {
                        _logger.LogWarning("Received message of unexpected type {MessageType}, skipping processing", messageType);
                        await args.DeadLetterMessageAsync(args.Message, "InvalidMessageType", $"Unexpected message type: {messageType}");
                        return;
                    }
                    var jsonObj = args.Message.Body.ToString();
                    if (messageType == nameof(InventoryCancelledEvent))
                    {

                        var cancelRequest = JsonSerializer.Deserialize<InventoryCancelledEvent>(jsonObj);
                        _logger.LogInformation("Processing {InventoryCancelledEvent} for OrderId {OrderId} json reserveRequest: {jsonObj}", nameof(InventoryCancelledEvent), cancelRequest?.OrderId, jsonObj);

                        if (cancelRequest != null)
                        {
                            _logger.LogInformation("Received InventoryCancelRequestEvent for OrderId {OrderId} json cancelRequest: {jsonObj}", cancelRequest.OrderId, jsonObj);
                            var success = _inventoryService.UnreserveInventory(cancelRequest);

                            if (success)
                            {
                                _logger.LogInformation("Published InventoryCancelledConfirmedEvent for OrderId {OrderId}", cancelRequest.OrderId);
                                await args.CompleteMessageAsync(args.Message);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to unreserve inventory for OrderId {OrderId}, skipping", cancelRequest.OrderId);
                                await args.AbandonMessageAsync(args.Message);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Received null InventoryCancelRequestEvent");
                            await args.AbandonMessageAsync(args.Message);
                        }

                        return;
                    }
                    else
                    {

                        var reserveRequest = JsonSerializer.Deserialize<InventoryRequestedEvent>(jsonObj);

                        _logger.LogInformation("Processing {InventoryRequestedEvent} for OrderId {OrderId} json reserveRequest: {jsonObj}", nameof(InventoryRequestedEvent), reserveRequest?.OrderId, jsonObj);

                        if (reserveRequest != null)
                        {
                            var success = _inventoryService.ReserveInventory(reserveRequest);

                            if (success)
                            {
                                _logger.LogInformation("Published InventoryReservedConfirmedEvent for OrderId {OrderId}", reserveRequest.OrderId);

                                var confirmed = new InventoryConfirmedEvent
                                {
                                    OrderId = reserveRequest.OrderId
                                };

                                await _inventoryService.SendInventoryConfirmedAsync(confirmed);
                            }
                            else
                            {
                                _logger.LogInformation("Published InventoryReservationFailedEvent for OrderId {OrderId}", reserveRequest.OrderId);

                                var rejected = new InventoryCheckFailedEvent
                                {
                                    OrderId = reserveRequest.OrderId,
                                    Reason = "Not enough stock"
                                };

                                await _inventoryService.SendInventoryRejectedAsync(rejected);
                            }

                            await args.CompleteMessageAsync(args.Message);
                        }
                        else
                        {
                            _logger.LogWarning("Received null InventoryReserveRequestEvent");
                            await args.AbandonMessageAsync(args.Message);
                        }
                    }
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
