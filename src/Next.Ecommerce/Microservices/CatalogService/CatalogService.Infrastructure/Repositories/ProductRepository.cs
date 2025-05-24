using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<ProductRepository> _logger;
    private readonly string _className = nameof(ProductRepository);

    public ProductRepository(CatalogDbContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            return await _context.Products.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_className}: Error occurred while retrieving all products.");
            throw; 
        }
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Products.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_className}: Error occurred while retrieving product with ID {id}.", id);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            return await _context.Products
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_className}: Error occurred while paginating products. Page: {pageNumber}, Size: {pageSize}", pageNumber, pageSize);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> SearchAsync(string query)
    {
        try
        {
            return await _context.Products
                .Where(p =>
                    (!string.IsNullOrWhiteSpace(p.Name) && p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(p.Description) && p.Description.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{_className}: Error occurred while searching products. Query: {query}", query);
            throw;
        }
    }
}