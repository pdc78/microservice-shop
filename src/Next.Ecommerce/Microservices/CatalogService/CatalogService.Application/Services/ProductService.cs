using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using Microsoft.Extensions.Logging;


namespace CatalogService.Application.Services;

public class ProductService : IProductSetrvice
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;
    private readonly string _className = nameof(ProductService);

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)

    {
        _productRepository = productRepository;
        _logger = logger;
    }
    
    public Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        
        try
        {
           return _productRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_className}: Failed to get products.", ex);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetPaginatedProductsAsync(int pageNumber, int pageSize)
    {
        try
        {
            return await _productRepository.GetPaginatedAsync(pageNumber,pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_className}: Failed to get paginated products. Page: {pageNumber}, Size: {pageSize}", pageNumber, pageSize);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        try
        {
            return await _productRepository.SearchAsync(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_className}: Failed to get search products {query}", query);
            throw;
        }
    }
}