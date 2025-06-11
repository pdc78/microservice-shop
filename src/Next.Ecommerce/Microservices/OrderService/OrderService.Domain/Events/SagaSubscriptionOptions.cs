namespace OrderService.Domain.Events
{

    public class SagaSubscriptionOptions
    {
        public List<SagaTopicSubscriptionConfiguration> SagaSubscriptions { get; set; }
    }
}
