namespace Contracts.Events;

// This class represents an event that is triggered when an order is created.
// It contains the order ID, user ID, and a list of items in the order.
public class OrderCreatedEvent : IIntegrationEvent
{
    public Guid OrderId { get; set; }
    public required string UserId { get; set; }
    public required List<OrderItemEvent> Items { get; set; }
    public required string ShippingAddress { get; set; }
    public decimal TotalAmount { get; set; }
}
