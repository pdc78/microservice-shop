// using Microsoft.AspNetCore.Mvc;
// using System.Net.Http;
// using System.Text.Json;
// using System.Threading.Tasks;
// using System.Collections.Generic;
// using Microsoft.AspNetCore.Mvc;
// using System.Net.Http;
// using System.Net.Http.Json;
// using System.Threading.Tasks;
// using System.Collections.Generic;
// using eShop.Models;

// namespace eshop.Controllers
// {
//     public class ProductsController : Controller
//     {
//         private readonly HttpClient _httpClient;
//         public ProductsController(HttpClient httpClient)
//         {
//             _httpClient = httpClient;
//         }
//     //     private static readonly List<Product> _products = new()
//         // {
//         //     new Product { Id = 1, Name = "Product A", Price = 10.5m },
//         //     new Product { Id = 2, Name = "Product B", Price = 20m },
//         //     new Product { Id = 3, Name = "Product C", Price = 15.3m }
//         // };

//         public async Task<IActionResult> Index(string? search)
//         {
//             // var filtered = string.IsNullOrWhiteSpace(search)
//             //     ? _products
//             //     : _products.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                
//                 // URL API Gateway
//             var apiUrl = "http://localhost:5096/apigateway/catalog/products";

//             // Chiama API e riceve lista prodotti
//             var filtered = await _httpClient.GetFromJsonAsync<List<Product>>(apiUrl);

//             ViewData["Search"] = search;


//             return View(filtered);
//         }
//     }
// }


using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using eShop.Models;

namespace eShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var apiUrl = "http://localhost:5096/apigateway/catalog/products";

            var products = await _httpClient.GetFromJsonAsync<List<Product>>(apiUrl);

            // Se vuoi filtrare in base a 'search'
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.FindAll(p => p.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewData["Search"] = search;

            return View(products);
        }
    }
}
