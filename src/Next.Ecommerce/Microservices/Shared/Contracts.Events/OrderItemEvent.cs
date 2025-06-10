namespace Contracts.Events;
public class OrderItemEvent
{
    public OrderItemEvent(Guid productId, int quantity)
    {
        this.ProductId = productId;
        this.Quantity = quantity;
    }

    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
