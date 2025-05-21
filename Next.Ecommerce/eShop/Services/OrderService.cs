
using eShop.ApiClients;
using eShop.Models;
using eShop.Services.Interfaces;

namespace eShop.Services;

public class OrderService : IOrderService
{
    private readonly ICatalogApiClient _catalogApiClient;

    public OrderService(ICatalogApiClient catalogApiClient)
    {
        _catalogApiClient = catalogApiClient;
    }

    public Task<List<OrderDto>> GetOrdersAsync()
    {
        return _catalogApiClient.GetAllOrdersAsync();
    }
}