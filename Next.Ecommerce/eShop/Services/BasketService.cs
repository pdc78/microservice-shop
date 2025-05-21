using eShop.ApiClients;
using eShop.Models;
using eShop.Services.Interfaces;

namespace eShop.Services
{
    public class BasketService : IBasketService
    {
        private readonly ICatalogApiClient _catalogApiClient;

        public BasketService(ICatalogApiClient catalogApiClient)
        {
            _catalogApiClient = catalogApiClient;
        }

        public async Task<BasketDto?> GetBasketAsync(string userId)
        {
            return await _catalogApiClient.GetBasketAsync(userId);
        }

        public async Task CreateBasketAsync(string userId)
        {
            var dto = new CreateBasketDto { UserId = userId };
            await _catalogApiClient.CreateBasketAsync(dto);
        }

        public async Task AddItemAsync(string userId, AddBasketItemDto item)
        {
            await _catalogApiClient.AddItemAsync(userId, item);
        }

        public async Task RemoveItemAsync(string userId, Guid productId)
        {
            await _catalogApiClient.RemoveItemAsync(userId, productId);
        }

        public async Task DeleteBasketAsync(string userId)
        {
            await _catalogApiClient.DeleteBasketAsync(userId);
        }
    }
}
