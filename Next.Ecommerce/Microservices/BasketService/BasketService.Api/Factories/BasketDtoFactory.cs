using BasketService.Api.DTOs;
using BasketService.Domain.Entities;

namespace BasketService.Api.Factories;
public static class BasketDtoFactory
{
    public static BasketDto Create(Basket basket)
    {
        return new BasketDto
        {
            UserId = basket.UserId,
            Items = basket.Items.Select(i => new BasketItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}