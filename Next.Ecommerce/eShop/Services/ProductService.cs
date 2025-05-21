
using eShop.ApiClients;
using eShop.Models;
using eShop.Services.Interfaces;

namespace eShop.Services;

public class ProductService : IProductService
{
    private readonly ICatalogApiClient _catalogApiClient;

    public ProductService(ICatalogApiClient catalogApiClient)
    {
        _catalogApiClient = catalogApiClient;
    }

    public async Task<List<ProductDto>> GetProductsAsync()
    {
        return await _catalogApiClient.GetAllProductsAsync();
    }
}