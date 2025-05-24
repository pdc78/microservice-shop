using CatalogService.Api.DTOs;
using CatalogService.Api.Extensions;
using CatalogService.Application.Exceptions;
using CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ValidationException = CatalogService.Application.Exceptions.ValidationException;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("api/catalog/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductSetrvice _productService;
        private readonly ILogger<ProductsController> _logger;
        public ProductsController(ILogger<ProductsController> logger, IProductSetrvice productService)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber <= 0 || pageSize <= 0)
                    throw new ArgumentOutOfRangeException("pageNumber", "Page number must be greater than zero.");

                var products = await _productService.GetPaginatedProductsAsync(pageNumber, pageSize);

                if (!products.Any())
                    throw new NotFoundException("No products found for the requested page.");

                var productDtos = products.Select(p => p.ToDto());
                return Ok(productDtos);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failed in GetProducts endpoint: {Message}", ex.Message);
                throw; // re-throw to be handled by the middleware
            }
            catch (NotFoundException ex)
            {
                _logger.LogInformation(ex, "No products found: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred in GetProducts endpoint.");
                throw;
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ValidationException("query","Query string cannot be empty.");


            var products = await _productService.SearchProductsAsync(query);
            if (!products.Any())
                throw new NotFoundException($"No products found matching: '{query}'");

            var productDtos = products.Select(p => p.ToDto());
            return Ok(productDtos);
        }
    }
}
