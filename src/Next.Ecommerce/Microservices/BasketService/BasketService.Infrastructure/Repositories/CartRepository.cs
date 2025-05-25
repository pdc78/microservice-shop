using BasketService.Application.Interfaces;
using BasketService.Domain.Entities;
using CatalogService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CatalogService.Infrastructure.Repositories;
public class BasketRepository : IBasketRepository
{
    private readonly BasketDbContext _context;
    private readonly ILogger<BasketRepository> _logger;
    public BasketRepository(ILogger<BasketRepository> logger, BasketDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Basket?> GetByUserIdAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching basket for user: {UserId}", userId);
            return await _context.Baskets
              .Include(b => b.Items)
              .FirstOrDefaultAsync(b => b.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving basket for user: {UserId}", userId);
            return null;
        }
    }
    public async Task SaveAsync(Basket basket)
    {
        var existing = await _context.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.UserId == basket.UserId);

        if (existing == null)
        {
            _context.Baskets.Add(basket);
        }
        else
        {
            existing.Items = basket.Items;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Deleting basket for user: {UserId}", userId);
            var basket = await GetByUserIdAsync(userId);
            if (basket != null)
            {
                _context.Baskets.Remove(basket);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Basket deleted successfully for user: {UserId}", userId);
            }
            else
            {
                _logger.LogWarning("Basket not found for user: {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting basket for user: {UserId}", userId);
        }
    }
}