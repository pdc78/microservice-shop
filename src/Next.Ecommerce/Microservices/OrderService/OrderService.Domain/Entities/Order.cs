namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(item => item.Quantity * item.UnitPrice);
    public string ShippingAddress { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}