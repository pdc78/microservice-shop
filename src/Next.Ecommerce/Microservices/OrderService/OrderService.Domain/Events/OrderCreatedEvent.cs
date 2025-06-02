namespace OrderService.Domain.Events;

// This class represents an event that is triggered when an order is created.
// It contains the order ID, user ID, and a list of items in the order.
public class OrderCreatedEvent
{
    public string OrderId { get; set; }
    public string UserId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}
