namespace OrderService.Domain.Events;

public class SagaOrderItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
