using eShop.Models;
using eShop.Strategies;

namespace eShop.ApiClients;
public class BasketApiClient : IBasketApiClient
{
    private readonly IHttpStrategy _http;
    private readonly ILogger<BasketApiClient> _logger;

    public BasketApiClient(IHttpStrategy http,ILogger<BasketApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public Task<BasketDto?> GetBasketAsync(string userId) =>
        _http.GetAsync<BasketDto>($"/apigateway/basket/{userId}");

    public Task AddItemAsync(string userId, AddBasketItemDto item) =>
        _http.PostAsync($"/apigateway/basket/{userId}/items", item);

    public Task RemoveItemAsync(string userId, Guid productId) =>
        _http.DeleteAsync($"/apigateway/basket/{userId}/items/{productId}");

    public Task DeleteBasketAsync(string userId) =>
        _http.DeleteAsync($"/apigateway/basket/{userId}");
}

