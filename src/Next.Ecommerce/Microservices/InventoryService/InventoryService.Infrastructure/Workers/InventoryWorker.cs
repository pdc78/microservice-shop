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
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _inventoryService = inventoryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Inventory Worker started");

            var processor = _serviceBusClient.CreateProcessor("InventoryTopic", new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var json = args.Message.Body.ToString();
                    var reserveRequest = JsonSerializer.Deserialize<InventoryReserveEvent>(json);

                    if (reserveRequest != null)
                    {
                        var success = await _inventoryService.ReserveInventoryAsync(reserveRequest);

                        if (success)
                        {
                            var confirmed = new InventoryReservedConfirmedEvent
                            {
                                OrderId = reserveRequest.OrderId
                            };

                            await _inventoryService.SendInventoryConfirmedAsync(confirmed);
                        }
                        else
                        {
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
                    _logger.LogError(ex, "Error processing inventory message");
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Service Bus error");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);
        }
    }
}
