using eShop.Models;
using eShop.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Controllers
{
    public class BasketController : Controller
    {
        private readonly IBasketService _basketService;

        public BasketController(IBasketService basketService)
        {
            _basketService = basketService;
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
            return RedirectToAction("Index", new { userId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(string userId, Guid productId)
        {
            await _basketService.RemoveItemAsync(userId, productId);
            return RedirectToAction("Index", new { userId });
        }
    }
}
