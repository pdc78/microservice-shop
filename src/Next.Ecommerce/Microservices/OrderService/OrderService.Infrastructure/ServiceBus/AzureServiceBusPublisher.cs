// File: Infrastructure/ServiceBus/AzureServiceBusPublisher.cs

using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Threading.Tasks;
using OrderService.Application.Interfaces;

namespace OrderService.Infrastructure.ServiceBus;

// This code defines an AzureServiceBusPublisher class that implements the IServiceBusPublisher interface.
// It uses the Azure.Messaging.ServiceBus library to send messages to a specified topic in Azure Service Bus.
public class AzureServiceBusPublisher : IServiceBusPublisher
{
    private readonly ServiceBusClient _client;

    public AzureServiceBusPublisher(ServiceBusClient client)
    {
        _client = client;
    }

    public async Task PublishAsync(string topicName, object message)
    {
        var sender = _client.CreateSender(topicName);
        var json = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(json);
        await sender.SendMessageAsync(serviceBusMessage);
    }
}