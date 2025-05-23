
using OrderService.Application.Interfaces;
using OrderService.Domain.DTOs;
using OrderService.Domain.Entities;

namespace OrderService.Application.Services;

public class OrderProcessingService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderProcessingService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> CreateOrderAsync(BasketDto basket)
    {
        var order = new Order
        {
            UserId = basket.UserId,
            Items = basket.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        await _repository.AddAsync(order);
        return order.Id;
    }
}
