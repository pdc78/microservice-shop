using eShop.Models;

namespace eShop.Services.Interfaces;
public interface IOrderService
{
    Task<OrderDto> CreateOrdersAsync(BasketDto basket);
}