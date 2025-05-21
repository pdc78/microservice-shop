using eShop.Models;

namespace eShop.ApiClients;

public interface ICatalogApiClient
{
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<List<OrderDto>> GetAllOrdersAsync();
}