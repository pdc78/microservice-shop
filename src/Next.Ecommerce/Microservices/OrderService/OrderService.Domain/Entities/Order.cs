namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
    public string Status { get; set; } = "Pending"; // Default status
    public decimal TotalAmount => Items.Sum(item => item.Quantity * item.UnitPrice);
}