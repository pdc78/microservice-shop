
using eShop.ApiClients;
using eShop.Models;
using eShop.Services.Interfaces;

namespace eShop.Services;

public class OrderService : IOrderService
{
    private readonly IOrderApiClient _orderApiClient;

    public OrderService(IOrderApiClient orderApiClient)
    {
        _orderApiClient = orderApiClient;
    }

    public Task<List<OrderDto>> GetOrdersAsync()
    {
        return _orderApiClient.GetAllOrdersAsync();
    }
}