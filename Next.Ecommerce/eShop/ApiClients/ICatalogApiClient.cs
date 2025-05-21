using eShop.Models;

namespace eShop.ApiClients;

public interface ICatalogApiClient
{
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<List<OrderDto>> GetAllOrdersAsync();
    Task<BasketDto?> GetBasketAsync(string userId);
    Task CreateBasketAsync(CreateBasketDto dto);
    Task AddItemAsync(string userId, AddBasketItemDto item);
    Task RemoveItemAsync(string userId, Guid productId);
    Task DeleteBasketAsync(string userId);
}