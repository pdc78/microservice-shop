
using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Domain.DTOs;
using OrderService.Domain.Entities;
using Contracts.Events;

namespace OrderService.Application.Services;

public class OrderProcessingService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IServiceBusPublisher _serviceBusPublisher;
    private readonly ILogger<OrderProcessingService> _logger;
    private readonly string _orderTopicName;

    public OrderProcessingService(IOrderRepository repository, IServiceBusPublisher serviceBusPublisher, string orderTopicName, ILogger<OrderProcessingService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(IOrderRepository));
        _serviceBusPublisher = serviceBusPublisher ?? throw new ArgumentNullException(nameof(IServiceBusPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(ILogger<OrderProcessingService>));
        _orderTopicName = string.IsNullOrEmpty(orderTopicName) ? throw new ArgumentNullException(orderTopicName) : orderTopicName;
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
            }).ToList(),
            ShippingAddress = "fake address"
        };

        await _repository.AddAsync(order);

        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            Items = order.Items.Select(i => new OrderItemEvent(i.ProductId, i.Quantity)).ToList(),
            ShippingAddress = order.ShippingAddress, // Replace with actual shipping address logic
            TotalAmount = order.TotalAmount
        };

        await _serviceBusPublisher.PublishAsync(_orderTopicName, orderCreatedEvent.OrderId.ToString(), nameof(OrderCreatedEvent), orderCreatedEvent);
        // Optionally, you can also publish an event to notify other services about the new order
        return order.Id;
    }
}
