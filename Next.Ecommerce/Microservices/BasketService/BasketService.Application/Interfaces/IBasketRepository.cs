using BasketService.Domain.Entities;

namespace BasketService.Application.Interfaces;
public interface IBasketRepository
{
    Task<Basket?> GetByUserIdAsync(string userId);
    Task AddOrUpdateAsync(Basket basket);
    Task DeleteAsync(string userId);
}