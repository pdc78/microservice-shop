using CatalogService.Domain.Entities;

namespace CatalogService.Application.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<IEnumerable<Product>> GetPaginatedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<Product>> SearchAsync(string query);
}