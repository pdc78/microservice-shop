namespace BasketService.Domain.Entities;

public class Basket
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = null!;
    public List<BasketItem> Items { get; set; } = new();
}