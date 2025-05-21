using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatalogService.Application.Services;

public class ProductService : IProductSetrvice
{
    private readonly IProductRepository _productRepository;
    
    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    public Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return _productRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        var allProducts = await _productRepository.GetAllAsync();
        return allProducts
            .Where(p =>
                (!string.IsNullOrWhiteSpace(p.Name) && p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(p.Description) && p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
            );
    }
}