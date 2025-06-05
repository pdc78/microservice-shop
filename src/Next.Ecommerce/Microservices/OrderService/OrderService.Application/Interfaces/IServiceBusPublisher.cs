namespace OrderService.Application.Interfaces;


// This interface defines a contract for publishing messages to a service bus.
// It includes a method to publish messages to a specified topic.
public interface IServiceBusPublisher
{
    Task PublishAsync(string topicName,string orderId, string messageType, object message);
}