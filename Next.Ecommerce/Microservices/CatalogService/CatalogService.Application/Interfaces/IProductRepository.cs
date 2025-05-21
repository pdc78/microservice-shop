using CatalogService.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatalogService.Application.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
}