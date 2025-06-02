// File: Infrastructure/ServiceBus/AzureServiceBusPublisher.cs

using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Threading.Tasks;
using OrderService.Application.Interfaces;
using CatalogService.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace OrderService.Infrastructure.ServiceBus;

// This code defines an AzureServiceBusPublisher class that implements the IServiceBusPublisher interface.
// It uses the Azure.Messaging.ServiceBus library to send messages to a specified topic in Azure Service Bus.
public class AzureServiceBusPublisher : IServiceBusPublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<AzureServiceBusPublisher> _logger;

    public AzureServiceBusPublisher(ServiceBusClient client,ILogger<AzureServiceBusPublisher> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(ServiceBusClient), "ServiceBusClient cannot be null");
        _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<AzureServiceBusPublisher>), "Logger cannot be null");
    }

    public async Task PublishAsync(string topicName, object message)
    {
        var sender = _client.CreateSender(topicName);
        var json = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(json);
        await sender.SendMessageAsync(serviceBusMessage);
    }
}