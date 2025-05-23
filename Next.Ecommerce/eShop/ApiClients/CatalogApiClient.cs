using eShop.Models;
using eShop.Strategies;

namespace eShop.ApiClients;

public class CatalogApiClient : ICatalogApiClient
{
    private readonly IHttpStrategy _httpStrategy;

    public CatalogApiClient(IHttpStrategy httpStrategy)
    {
        _httpStrategy = httpStrategy;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        return await _httpStrategy.GetAsync<List<ProductDto>>("/apigateway/catalog/products") 
            ?? new List<ProductDto>(); ;
    }
}