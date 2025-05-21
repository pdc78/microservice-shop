using CatalogService.Api.DTOs;
using CatalogService.Api.Extensions;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Services;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers
{
    [ApiController]
    [Route("api/catalog/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductSetrvice _productService;
        public ProductsController(IProductSetrvice productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Page number and page size must be greater than zero.");

            var products = await _productService.GetPaginatedProductsAsync(pageNumber, pageSize);
            var productDtos = products.Select(p => p.ToDto());

            return Ok(productDtos);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query string cannot be empty.");

            var products = await _productService.SearchProductsAsync(query);
            var productDtos = products.Select(p => p.ToDto());

            return Ok(productDtos);
        }

    }
}
