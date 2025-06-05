namespace OrderService.Domain.Events;
public abstract class ShippingEvent: IIntegrationEvent
{
    public Guid OrderId { get; init; }
}

public sealed class ShippingRequestedEvent : ShippingEvent
{
    public required string Address { get; init; }
}

public sealed class ShippingConfirmedEvent : ShippingEvent
{
    // No additional properties needed
}

public sealed class ShippingFailedEvent : ShippingEvent
{
    public required string Reason { get; init; }
}

public sealed class ShippingCancelledEvent : ShippingEvent
{
    public required string Reason { get; init; }
}
