namespace OrderService.Domain.Events;
public abstract class InventoryEvent : IIntegrationEvent
{
    public Guid OrderId { get; init; }
}

public sealed class InventoryRequestedEvent : InventoryEvent
{
    public required List<OrderItemEvent> Items { get; init; }
}

public sealed class InventoryCancelledEvent : InventoryEvent
{
    public required List<OrderItemEvent> Items { get; init; }
    public required string Reason { get; init; }
}

public sealed class InventoryConfirmedEvent : InventoryEvent
{
    // No extra properties needed; inherits OrderId
}

public sealed class InventoryCheckFailedEvent : InventoryEvent
{
    public required string Reason { get; init; }
}
