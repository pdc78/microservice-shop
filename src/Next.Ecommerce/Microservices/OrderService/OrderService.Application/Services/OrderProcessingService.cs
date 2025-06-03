
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Domain.DTOs;
using OrderService.Domain.Entities;
using OrderService.Domain.Events;

namespace OrderService.Application.Services;

public class OrderProcessingService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IServiceBusPublisher _bus;
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(IOrderRepository repository, IServiceBusPublisher bus, ILogger<OrderProcessingService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(IOrderRepository));
        _bus = bus ?? throw new ArgumentNullException(nameof(IServiceBusPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<OrderProcessingService>));
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
        
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = basket.UserId,
            Items = basket.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        await _bus.PublishAsync("OrderTopic", nameof(OrderCreatedEvent), orderCreatedEvent);
        // Optionally, you can also publish an event to notify other services about the new order
        return order.Id;
    }
}
