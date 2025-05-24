using CatalogService.Api.DTOs;
using CatalogService.Domain.Entities;

namespace CatalogService.Api.Extensions;
public static class ProductExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price
        };
    }
}