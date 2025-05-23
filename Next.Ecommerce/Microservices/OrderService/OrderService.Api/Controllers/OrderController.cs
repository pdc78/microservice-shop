using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Interfaces;
using OrderService.Domain.DTOs;

namespace BasketService.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] BasketDto basket)
    {
        var orderId = await _orderService.CreateOrderAsync(basket);
        var result= CreatedAtAction(nameof(GetOrder), new { id = orderId }, new { OrderId = orderId });
        return result;
    }

    [HttpGet("{id}")]
    public IActionResult GetOrder(Guid id)
    {
        // Optional placeholder for retrieving order
        return Ok(new { Message = $"Order {id} created successfully." });
    }
}