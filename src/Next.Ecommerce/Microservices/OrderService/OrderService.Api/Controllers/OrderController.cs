using Microsoft.AspNetCore.Mvc;
using OrderService.Api.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.DTOs;

namespace BasketService.Api.Controllers;

[ApiController]
[Route("api/order")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] BasketDto basket)
    {
        if (basket == null || string.IsNullOrWhiteSpace(basket.UserId) || basket.Items == null || !basket.Items.Any())
        {
            return BadRequest("Invalid basket data.");
        }

        try
        {
            var orderId = await _orderService.CreateOrderAsync(basket);
            return Ok(new OrderDto { Id = orderId });
        }
        catch (Exception ex)
        {
            // Log the error (you can use ILogger if configured)
            return StatusCode(500, $"An error occurred while creating the order: {ex.Message}");
        }
    }
}