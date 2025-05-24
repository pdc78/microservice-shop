using OrderService.Domain.DTOs;

namespace OrderService.Application.Interfaces;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(BasketDto basket);
}
