using eShop.ApiClients;
using eShop.Models;
using eShop.Services.Interfaces;

namespace eShop.Services
{
    public class BasketService : IBasketService
    {
        private readonly IBasketApiClient _basketApiClient;

        public BasketService(IBasketApiClient catalogApiClient)
        {
            _basketApiClient = catalogApiClient;
        }

        public async Task<BasketDto?> GetBasketAsync(string userId)
        {
            return await _basketApiClient.GetBasketAsync(userId);
        }

        public async Task AddItemAsync(string userId, AddBasketItemDto item)
        {
            await _basketApiClient.AddItemAsync(userId, item);
        }

        public async Task RemoveItemAsync(string userId, Guid productId)
        {
            await _basketApiClient.RemoveItemAsync(userId, productId);
        }

        public async Task DeleteBasketAsync(string userId)
        {
            await _basketApiClient.DeleteBasketAsync(userId);
        }
    }
}
