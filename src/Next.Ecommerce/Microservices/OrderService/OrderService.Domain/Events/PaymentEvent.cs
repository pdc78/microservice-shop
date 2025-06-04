namespace OrderService.Domain.Events;

public class PaymentRequestEvent
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentConfirmedEvent
{
    public Guid OrderId { get; set; }
}

public class PaymentFailedEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}