namespace CatalogService.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
  
    public required string Description { get; set; }

    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
}