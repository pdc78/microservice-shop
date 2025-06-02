// File: Application/Interfaces/IServiceBusPublisher.cs

using System.Threading.Tasks;

namespace OrderService.Application.Interfaces;


// This interface defines a contract for publishing messages to a service bus.
// It includes a method to publish messages to a specified topic.
public interface IServiceBusPublisher
{
    Task PublishAsync(string topicName, object message);
}