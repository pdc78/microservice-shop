using Microsoft.AspNetCore.Mvc;
using eShop.Services.Interfaces;
using eShop.ViewModels;

namespace eShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;
        private readonly IBasketService _basketService; // You need to add this service
        private readonly string userId = "pier";
        public ProductController(ILogger<ProductController> logger, IProductService productService, IBasketService basketService)
        {
            _logger = logger;
            _productService = productService;
            _basketService = basketService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetProductsAsync();
                var basket = await _basketService.GetBasketAsync(userId);

                var model = new ProductCatalogViewModel
                {
                    Products = products,
                    Basket = basket,
                    UserId = userId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Failed to fetch products";
                _logger.LogError(ex, "Error loading products");
                return View("Error");
            }
        }
    }
}
