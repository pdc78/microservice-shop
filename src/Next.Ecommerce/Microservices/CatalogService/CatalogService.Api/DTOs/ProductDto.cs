using System.ComponentModel.DataAnnotations;

namespace CatalogService.Api.DTOs;
public class ProductDto
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public string Description { get; set; }
    [Required]
    public decimal Price { get; set; }
}
