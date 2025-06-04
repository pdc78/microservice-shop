namespace OrderService.Domain.Events;

public class ShippingRequestEvent
{
    public Guid OrderId { get; set; }
    public string Address { get; set; }
}

public class ShippingConfirmedEvent
{
    public Guid OrderId { get; set; }
}

public class ShippingFailedEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}