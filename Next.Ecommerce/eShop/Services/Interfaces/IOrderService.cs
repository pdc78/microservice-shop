using eShop.Models;

namespace eShop.Services.Interfaces;
public interface IOrderService
{
    Task<List<OrderDto>> GetOrdersAsync();
}