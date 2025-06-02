namespace OrderService.Domain.Events;

// This class represents an item in an order, containing the product ID and quantity.
// It is used in the context of order creation and processing within the Order Service domain.
public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
