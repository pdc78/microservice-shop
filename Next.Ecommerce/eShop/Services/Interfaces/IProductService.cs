using eShop.Models;

namespace eShop.Services.Interfaces;
public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync();
}