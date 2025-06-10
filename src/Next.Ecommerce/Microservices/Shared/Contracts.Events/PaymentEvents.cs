namespace Contracts.Events;
public abstract class PaymentEvent : IIntegrationEvent
{
    public Guid OrderId { get; init; }
}

public sealed class PaymentRequestedEvent : PaymentEvent
{
    public decimal Amount { get; init; }
}

public sealed class PaymentConfirmedEvent : PaymentEvent
{
    // No additional properties needed
}

public sealed class PaymentFailedEvent : PaymentEvent
{
    public required string Reason { get; init; }
}

public sealed class PaymentCancelledEvent : PaymentEvent
{
    public required string Reason { get; init; }
}
