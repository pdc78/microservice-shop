using eShop.Models;
using eShop.Strategies;

namespace eShop.ApiClients
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly IHttpStrategy _httpStrategy;

        public OrderApiClient(IHttpStrategy httpStrategy)
        {
            _httpStrategy = httpStrategy;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            return await _httpStrategy.GetAsync<List<OrderDto>>("/apigateway/orders") ?? new List<OrderDto>();
        }

        public Task CreateOrderAsync(CreateBasketDto dto)
        {
            return _httpStrategy.PostAsync("/orders", dto);
        }
    }
}
