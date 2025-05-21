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
    public BasketRepository(ILogger<BasketRepository> logger,BasketDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Basket?> GetByUserIdAsync(string userId)
    {
        return await _context.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.UserId == userId);
    }

    public async Task AddOrUpdateAsync(Basket basket)
    {
        var existing = await _context.Baskets
          .Include(b => b.Items)
          .FirstOrDefaultAsync(b => b.UserId == basket.UserId);

        if (existing == null)
        {
            _context.Baskets.Add(basket); // new basket
        }
        else
        {
            foreach (var newItem in basket.Items)
            {
                var existingItem = existing.Items
                    .FirstOrDefault(i => i.ProductId == newItem.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += newItem.Quantity;
                }
                else
                {
                    // Do NOT reuse incoming BasketItem – recreate it
                    existing.Items.Add(new BasketItem
                    {
                        ProductId = newItem.ProductId,
                        ProductName = newItem.ProductName,
                        Quantity = newItem.Quantity,
                        UnitPrice = newItem.UnitPrice,
                        BasketId = existing.Id
                    });
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string userId)
    {
        var basket = await GetByUserIdAsync(userId);
        if (basket != null)
        {
            _context.Baskets.Remove(basket);
            await _context.SaveChangesAsync();
        }
    }
}