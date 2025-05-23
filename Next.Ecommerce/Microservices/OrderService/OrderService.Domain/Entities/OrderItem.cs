namespace OrderService.Domain.Entities;
public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; } // Foreign Key
    public Order Order { get; set; } = default!;
    public string ProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}