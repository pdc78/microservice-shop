using BasketService.Domain.Entities;

namespace BasketService.Application.Interfaces;
public interface ICartService
{
    Task<Basket> AddItemAsync(string userId, BasketItem item);
    Task<Basket?> GetBasketAsync(string userId);
    Task RemoveItemAsync(string userId, Guid productId);
    Task DeleteBasketAsync(string userId);
}
