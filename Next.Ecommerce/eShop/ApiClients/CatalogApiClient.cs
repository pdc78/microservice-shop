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

    public async Task<BasketDto?> GetBasketAsync(string userId)
    {
        return await _httpClient.GetFromJsonAsync<BasketDto>($"apigateway/basket/{userId}");
    }

    public async Task CreateBasketAsync(CreateBasketDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("apigateway/basket", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task AddItemAsync(string userId, AddBasketItemDto item)
    {
        var response = await _httpClient.PostAsJsonAsync($"apigateway/basket/{userId}/items", item);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveItemAsync(string userId, Guid productId)
    {
        var response = await _httpClient.DeleteAsync($"apigateway/basket/{userId}/items/{productId}");
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteBasketAsync(string userId)
    {
        var response = await _httpClient.DeleteAsync($"apigateway/basket/{userId}");
        response.EnsureSuccessStatusCode();
    }
}