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
                var model = new ProductCatalogViewModel
                {
                    Products = products,
                    Basket = null
                };

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Failed to fetch products";
                _logger.LogError(ex.Message, ex);
                return View("Error");
                throw;
            }
        }
        public async Task<IActionResult> Index2()
        {
            try
            {
                var products = await _productService.GetProductsAsync();

                // Fetch basket for the user (replace with actual user ID logic)
                var basket = await _basketService.GetBasketAsync("user123");

                var model = new ProductCatalogViewModel
                {
                    Products = products,
                    Basket = basket
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
