using eShop.Models;

namespace eShop.Services.Interfaces;

public interface IBasketService
    {
        Task<BasketDto?> GetBasketAsync(string userId);
        Task CreateBasketAsync(string userId);
        Task AddItemAsync(string userId, AddBasketItemDto item);
        Task RemoveItemAsync(string userId, Guid productId);
        Task DeleteBasketAsync(string userId);
    }