using eShop.Models;

namespace eShop.ApiClients;

public class CatalogApiClient : ICatalogApiClient
{
    private readonly HttpClient _httpClient;

    public CatalogApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ProductDto>>("/apigateway/catalog/products")
               ?? new List<ProductDto>();
    }

    public async Task<List<OrderDto>> GetAllOrdersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<OrderDto>>("/apigateway/orders")
               ?? new List<OrderDto>();
    }
}