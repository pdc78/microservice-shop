using System.ComponentModel.DataAnnotations;

namespace CatalogService.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
}