using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using Microsoft.Extensions.Logging;
using OrderService.Infrastructure.Data;

namespace CatalogService.Infrastructure.Repositories;
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;
    private readonly ILogger<OrderRepository> _logger;
    public OrderRepository(OrderDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task<Order?> UpdateAsync(Guid Id, OrderStatus status)
    {
        var order = await _context.Orders.FindAsync(Id);
        if (order == null)
        {
            return null;
        }
        order.Status = status;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Order {OrderId} updated to status {Status}", Id, status);
        return order;
    }
}