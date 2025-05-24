using eShop.Models;

namespace eShop.ApiClients;
public interface IOrderApiClient
{
    Task<OrderDto> CreateOrderAsync(BasketDto basket);
}