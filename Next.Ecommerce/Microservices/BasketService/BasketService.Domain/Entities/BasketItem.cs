namespace BasketService.Domain.Entities;

public class BasketItem
{
    public int Id { get; set; } // Primary Key
    public Guid BasketId { get; set; } // Foreign Key
    public Basket Basket { get; set; } = default!;

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}