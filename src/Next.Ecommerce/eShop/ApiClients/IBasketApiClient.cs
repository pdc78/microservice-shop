using eShop.Models;

namespace eShop.ApiClients;
public interface IBasketApiClient : IApiClient
{
    Task<BasketDto?> GetBasketAsync(string userId);
    Task AddItemAsync(string userId, AddBasketItemDto item);
    Task RemoveItemAsync(string userId, Guid productId);
    Task DeleteBasketAsync(string userId);
}