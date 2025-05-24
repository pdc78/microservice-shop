using CatalogService.Domain.Entities;

namespace CatalogService.Application.Interfaces
{
    public interface IProductSetrvice
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<IEnumerable<Product>> SearchProductsAsync(string query);
        Task<IEnumerable<Product>> GetPaginatedProductsAsync(int pageNumber,int pageSize);
    }
}
