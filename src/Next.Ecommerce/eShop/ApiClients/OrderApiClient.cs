using eShop.Models;
using eShop.Strategies;
using static System.Net.WebRequestMethods;

namespace eShop.ApiClients
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly IHttpStrategy _httpStrategy;
        private readonly ILogger<OrderApiClient> _logger;

        public OrderApiClient(IHttpStrategy httpStrategy, ILogger<OrderApiClient> logger)
        {
            _httpStrategy = httpStrategy;
            _logger = logger;
        }

        public async Task<OrderDto?> CreateOrderAsync(BasketDto basket)
        {
            try
            {
                return await _httpStrategy.PostAsync<BasketDto, OrderDto>("/apigateway/order", basket);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(OrderApiClient)}: Error during API call", e);
                throw;
            }
        }
    }
}
