using Microsoft.AspNetCore.Mvc;
using eShop.Services.Interfaces;

namespace eShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;

        public ProductController(ILogger<HomeController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Failed to fetch products";
                _logger.LogError(ex.Message, ex);
                return View("Error");
                throw;
            }
        }
    }
}
