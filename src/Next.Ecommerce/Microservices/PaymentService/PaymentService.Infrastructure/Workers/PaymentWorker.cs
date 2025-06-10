using Azure.Messaging.ServiceBus;
using PaymentService.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Contracts.Events;

namespace PaymentService.Infrastructure.Workers
{
    public class PaymentWorker : BackgroundService
    {
        private readonly ILogger<PaymentWorker> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IPaymentService _PaymentService;

        public PaymentWorker(
            ILogger<PaymentWorker> logger,
            ServiceBusClient serviceBusClient,
            IPaymentService PaymentService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _PaymentService = PaymentService ?? throw new ArgumentNullException(nameof(PaymentService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Worker started");

            var processor = _serviceBusClient.CreateProcessor("paymenttopic", "payment-sub-request", new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var messageType = args.Message.ApplicationProperties.TryGetValue("messageType", out var type) ? type.ToString() : string.Empty;
                    _logger.LogInformation($"Received message of type {messageType} possible values {nameof(PaymentRequestedEvent)} - {nameof(PaymentCancelledEvent)}");
                    if (messageType != nameof(PaymentRequestedEvent) && messageType != nameof(PaymentCancelledEvent))
                    {
                        _logger.LogWarning("Received message of unexpected type {MessageType}, skipping processing", messageType);
                        await args.DeadLetterMessageAsync(args.Message, "InvalidMessageType", $"Unexpected message type: {messageType}");
                        return;
                    }
                    var jsonObj = args.Message.Body.ToString();
                    if (messageType == nameof(PaymentCancelledEvent))
                    {

                        var cancelRequest = JsonSerializer.Deserialize<PaymentCancelledEvent>(jsonObj);
                        _logger.LogInformation("Processing {PaymentCancelledEvent} for OrderId {OrderId} json reserveRequest: {jsonObj}", nameof(PaymentCancelledEvent), cancelRequest?.OrderId, jsonObj);

                        if (cancelRequest != null)
                        {
                            _logger.LogInformation("Received PaymentCancelRequestEvent for OrderId {OrderId} json cancelRequest: {jsonObj}", cancelRequest.OrderId, jsonObj);
                            var success = _PaymentService.CancelPayment(cancelRequest);

                            if (success)
                            {
                                _logger.LogInformation("Published PaymentCancelledConfirmedEvent for OrderId {OrderId}", cancelRequest.OrderId);
                                await args.CompleteMessageAsync(args.Message);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to canecellation Payment for OrderId {OrderId}, skipping", cancelRequest.OrderId);
                                await args.AbandonMessageAsync(args.Message);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Received null PaymentCancelRequestEvent");
                            await args.AbandonMessageAsync(args.Message);
                        }

                        return;
                    }
                    else
                    {
                        var reserveRequest = JsonSerializer.Deserialize<PaymentRequestedEvent>(jsonObj);

                        _logger.LogInformation("Processing {PaymentRequestedEvent} for OrderId {OrderId} json reserveRequest: {jsonObj}", nameof(PaymentRequestedEvent), reserveRequest?.OrderId, jsonObj);

                        if (reserveRequest != null)
                        {
                            var success = true;

                            if (success)
                            {
                                _logger.LogInformation("Published PaymentReservedConfirmedEvent for OrderId {OrderId}", reserveRequest.OrderId);

                                var confirmed = new PaymentConfirmedEvent
                                {
                                    OrderId = reserveRequest.OrderId
                                };

                                await _PaymentService.SendPaymentConfirmedAsync(confirmed);
                            }
                            else
                            {
                                _logger.LogInformation("Published PaymentReservationFailedEvent for OrderId {OrderId}", reserveRequest.OrderId);

                                var rejected = new PaymentFailedEvent
                                {
                                    OrderId = reserveRequest.OrderId,
                                    Reason = "No money on the CC"
                                };

                                await _PaymentService.SendPaymentRejectedAsync(rejected);
                            }

                            await args.CompleteMessageAsync(args.Message);
                        }
                        else
                        {
                            _logger.LogWarning("Received null PaymentReserveRequestEvent");
                            await args.AbandonMessageAsync(args.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{nameof(PaymentWorker)} : Error processing Payment message");
                }
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, $"{nameof(PaymentWorker)} Service Bus error");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(stoppingToken);
        }
    }
}
