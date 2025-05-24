using System.Net.Http;
using eShop.Models;
using eShop.Services.Interfaces;
using eShop.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Controllers
{
    public class BasketController : Controller
    {
        private readonly IBasketService _basketService;
        private readonly IOrderService _orderService;
        private readonly ILogger<BasketController> _logger;
        public BasketController(IBasketService basketService, IOrderService orderService, ILogger<BasketController> logger)
        {
            _basketService = basketService;
            _orderService = orderService;
            _logger = logger;   
        }

        public async Task<IActionResult> Index(string userId)
        {
            var basket = await _basketService.GetBasketAsync(userId);
            return View(basket);
        }

        [HttpPost]
        public async Task<IActionResult> AddItem(string userId, Guid productId, string productName, decimal unitPrice)
        {
            var item = new AddBasketItemDto
            {
                ProductId = productId,
                ProductName = productName,
                Quantity = 1,
                UnitPrice = unitPrice
            };

            await _basketService.AddItemAsync(userId, item);
            return RedirectToAction("Index", "Product", new { userId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(string userId, Guid productId)
        {
            await _basketService.RemoveItemAsync(userId, productId);
            return RedirectToAction("Index", "Product", new { userId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBasket(string userId)
        {
            await _basketService.DeleteBasketAsync(userId);
            return RedirectToAction("Index", "Product", new { userId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(string userId)
        {
            var basket = await _basketService.GetBasketAsync(userId);

            if (basket == null || basket.Items.Count == 0)
            {
                TempData["Error"] = "Basket is empty!";
                return RedirectToAction("Index", "Basket");
            }
            _logger.LogInformation($"{nameof(BasketController)} calling CreateOrdersAsync");
            var order = await _orderService.CreateOrdersAsync(basket);
            _logger.LogInformation($"{nameof(BasketController)} after call CreateOrdersAsync");
            if (order.Id != Guid.Empty)
            {
                var viewModel = new OrderConfirmationViewModel
                {
                    OrderId = order.Id.ToString(),
                    UserId = userId
                };

                return RedirectToAction("Index", "Order",viewModel);
            }
            else
            {
                TempData["Error"] = "Failed to create order.";
                return RedirectToAction("Index", "Basket");
            }
        }
    }
}
