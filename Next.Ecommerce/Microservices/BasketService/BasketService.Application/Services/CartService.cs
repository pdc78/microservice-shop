using BasketService.Application.Interfaces;
using BasketService.Domain.Entities;

namespace BasketService.Application.Services;

public class CartService : ICartService
{
    private readonly IBasketRepository _repository;
    public CartService(IBasketRepository repository)
    {
        _repository = repository;
    }


    public async Task<Basket> AddItemAsync(string userId, BasketItem item)
    {
        var basket = await _repository.GetByUserIdAsync(userId) ?? new Basket { UserId = userId };
        basket.Items.Add(item);

        await _repository.AddOrUpdateAsync(basket);
        return basket;
    }

    public async Task<Basket?> GetBasketAsync(string userId)
    {
        var basket = await _repository.GetByUserIdAsync(userId) ?? new Basket { UserId = userId };
        return basket;
    }

    public async Task RemoveItemAsync(string userId, Guid productId)
    {
        var basket = await _repository.GetByUserIdAsync(userId);
        if (basket == null) return;

        var item = basket.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            basket.Items.Remove(item);
            await _repository.AddOrUpdateAsync(basket);
        }
    }

    public Task DeleteBasketAsync(string userId) => _repository.DeleteAsync(userId);
}

