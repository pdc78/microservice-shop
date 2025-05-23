using eShop.Models;

namespace eShop.ApiClients;
public interface IOrderApiClient
{
    Task<List<OrderDto>> GetAllOrdersAsync();
    Task CreateOrderAsync(CreateBasketDto dto);
}