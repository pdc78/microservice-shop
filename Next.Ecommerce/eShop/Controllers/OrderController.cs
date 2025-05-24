using Microsoft.AspNetCore.Mvc;
using eShop.ViewModels;
using eShop.Services.Interfaces;

namespace eShop.Controllers;

[Route("[controller]")]
public class OrderController : Controller
{
    private readonly ILogger<OrderController> _logger;
    private readonly IBasketService _basketService;
    public OrderController(IBasketService basketService, ILogger<OrderController> logger)
    {
        _logger = logger;
        _basketService = basketService;
    }

    [HttpGet("Index")]
    public IActionResult Index(OrderConfirmationViewModel viewModel)
    {
        _logger.LogInformation($"Displaying order confirmation for OrderId: {viewModel.OrderId}, UserId: {viewModel.UserId}");
        

        if (string.IsNullOrEmpty(viewModel.OrderId))
        {
            TempData["Error"] = "Invalid order details.";
            return RedirectToAction("Index", "Basket");
        }
        _basketService.DeleteBasketAsync(viewModel.UserId);

        return View(viewModel);
    }
}
