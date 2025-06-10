using Azure.Messaging.ServiceBus;
using ShippingService.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Contracts.Events;

namespace ShippingService.Infrastructure.Workers
{
    public class ShippingWorker : BackgroundService
    {
        private readonly ILogger<ShippingWorker> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IShippingService _ShippingService;

        public ShippingWorker(
            ILogger<ShippingWorker> logger,
            ServiceBusClient serviceBusClient,
            IShippingService ShippingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _ShippingService = ShippingService ?? throw new ArgumentNullException(nameof(ShippingService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Shipping Worker started");

            var processor = _serviceBusClient.CreateProcessor("shippingtopic", "shipping-sub-request", new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var messageType = args.Message.ApplicationProperties.TryGetValue("messageType", out var type) ? type.ToString() : string.Empty;
                    _logger.LogInformation($"Received message of type {messageType} possible values {nameof(ShippingRequestedEvent)} - {nameof(ShippingCancelledEvent)}");
                    if (messageType != nameof(ShippingRequestedEvent) && messageType != nameof(ShippingCancelledEvent))
                    {
                        _logger.LogWarning("Received message of unexpected type {MessageType}, skipping processing", messageType);
                        await args.DeadLetterMessageAsync(args.Message, "InvalidMessageType", $"Unexpected message type: {messageType}");
                        return;
                    }
                    var jsonObj = args.Message.Body.ToString();
                    if (messageType == nameof(ShippingCancelledEvent))
                    {

                        var cancelRequest = JsonSerializer.Deserialize<ShippingCancelledEvent>(jsonObj);
                        _logger.LogInformation("Processing {ShippingCancelledEvent} for OrderId {OrderId} json reserveRequest: {jsonObj}", nameof(ShippingCancelledEvent), cancelRequest?.OrderId, jsonObj);

                        if (cancelRequest != null)
                        {
                            _logger.LogInformation("Received ShippingCancelRequestEvent for OrderId {OrderId} json cancelRequest: {jsonObj}", cancelRequest.OrderId, jsonObj);
                            var success = _ShippingService.CancelShipping(cancelRequest);

                            if (success)
                            {
                                _logger.LogInformation("Published ShippingCancelledConfirmedEvent for OrderId {OrderId}", cancelRequest.OrderId);
                                await args.CompleteMessageAsync(args.Message);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to canecellation Shipping for OrderId {OrderId}, skipping", cancelRequest.OrderId);
                                await args.AbandonMessageAsync(args.Message);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Received null ShippingCancelRequestEvent");
                            await args.AbandonMessageAsync(args.Message);
                        }

                        return;
                    }
                    else
                    {
                        var reserveRequest = JsonSerializer.Deserialize<ShippingRequestedEvent>(jsonObj);

                        _logger.LogInformation("Processing {ShippingRequestedEvent} for OrderId {OrderId} json reserveRequest: {jsonObj}", nameof(ShippingRequestedEvent), reserveRequest?.OrderId, jsonObj);

                        if (reserveRequest != null)
                        {
                            var success = true;

                            if (success)
                            {
                                _logger.LogInformation("Published ShippingReservedConfirmedEvent for OrderId {OrderId}", reserveRequest.OrderId);

                                var confirmed = new ShippingConfirmedEvent
                                {
                                    OrderId = reserveRequest.OrderId
                                };

                                await _ShippingService.SendShippingConfirmedAsync(confirmed);
                            }
                            else
                            {
                                _logger.LogInformation("Published ShippingReservationFailedEvent for OrderId {OrderId}", reserveRequest.OrderId);

                                var rejected = new ShippingFailedEvent
                                {
                                    OrderId = reserveRequest.OrderId,
                                    Reason = "Address Error"
                                };

                                await _ShippingService.SendShippingRejectedAsync(rejected);
                            }

                            await args.CompleteMessageAsync(args.Message);
                        }
                        else
                        {
                            _logger.LogWarning("Received null ShippingReserveRequestEvent");
                            await args.AbandonMessageAsync(args.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{nameof(ShippingWorker)} : Error processing Shipping message");
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, $"{nameof(ShippingWorker)} Service Bus error");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);
        }
    }
}
