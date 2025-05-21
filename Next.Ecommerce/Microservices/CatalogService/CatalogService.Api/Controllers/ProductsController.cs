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
        public async Task<ActionResult<IEnumerable<ProductDto>>> Get()
        {
            var products = await _productService.GetAllProductsAsync();
            var productDtos = new List<ProductDto>();
            foreach (var p in products)
            {
                productDtos.Add(p.ToDto());
            }
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
