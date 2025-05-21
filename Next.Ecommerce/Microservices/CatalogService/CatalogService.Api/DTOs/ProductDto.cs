using System.ComponentModel.DataAnnotations;

namespace CatalogService.Api.DTOs;
public class ProductDto
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    public decimal Price { get; set; }
}
