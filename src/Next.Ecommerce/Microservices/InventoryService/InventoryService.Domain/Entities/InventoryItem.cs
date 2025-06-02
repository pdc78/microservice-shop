namespace InventoryService.Domain.Entities;

public class InventoryItem
{
    public Guid ProductId { get; set; }
    public int AvailableQuantity { get; set; }
}