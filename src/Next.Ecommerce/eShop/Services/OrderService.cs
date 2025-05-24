
using eShop.ApiClients;
using eShop.Models;
using eShop.Services.Interfaces;

namespace eShop.Services;

public class OrderService : IOrderService
{
    private readonly IOrderApiClient _orderApiClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderApiClient orderApiClient, ILogger<OrderService> logger)
    {
        _orderApiClient = orderApiClient;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrdersAsync(BasketDto basket)
    {
        _logger.LogInformation($"{nameof(OrderService)} calling CreateOrderAsync");
        return await _orderApiClient.CreateOrderAsync(basket);
    }
}