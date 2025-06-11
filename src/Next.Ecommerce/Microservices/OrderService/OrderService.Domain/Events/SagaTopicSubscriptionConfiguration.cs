using Azure.Messaging.ServiceBus;

namespace OrderService.Domain.Events;
public class SagaTopicSubscriptionConfiguration
{
    public string Topic { get; set; } = default!;
    public string Subscription { get; set; } = default!;
    public Func<ProcessMessageEventArgs, Task> Handler { get; set; } = default!;
}
