using BasketService.Api.DTOs;
using BasketService.Api.Factories;
using BasketService.Application.Interfaces;
using BasketService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BasketService.Api.Controllers;

[ApiController]
[Route("api/basket")]
public class BasketController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<BasketController> _logger;
    public BasketController(ILogger<BasketController> logger, ICartService cartService)
    {
        _cartService = cartService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<Basket>> CreateBasket([FromBody] CreateBasketDto dto)
    {
        var basket = await _cartService.CreateBasketAsync(dto.UserId);
        var basketDto = BasketDtoFactory.Create(basket);
        return Ok(dto);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<Basket>> GetBasket(string userId)
    {
        try
        {
            var basket = await _cartService.GetBasketAsync(userId);
            if (basket == null)
                return NotFound();

            var dto = BasketDtoFactory.Create(basket);
            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve basket for user {UserId}", userId);
            return StatusCode(500, "An internal error occurred.");
        }

    }

    [HttpPost("{userId}/items")]
    public async Task<IActionResult> AddItem(string userId, [FromBody] AddBasketItemDto dto)
    {
        var item = new BasketItem
        {
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice
        };

        await _cartService.AddItemAsync(userId, item);
        return NoContent();
    }

    [HttpDelete("{userId}/items/{productId}")]
    public async Task<IActionResult> RemoveItem(string userId, Guid productId)
    {
        await _cartService.RemoveItemAsync(userId, productId);
        return NoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteBasket(string userId)
    {
        await _cartService.DeleteBasketAsync(userId);
        return NoContent();
    }
}
