using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using eShop.Models;
using System.Text.Json;

namespace eShop.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiGatewayUrl;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _apiGatewayUrl = configuration["ApiGateway:BaseUrl"] ?? Environment.GetEnvironmentVariable("API_GATEWAY_URL");
    }

    public async Task<IActionResult>  Index()
    {
          var client = _httpClientFactory.CreateClient();

        var response = await client.GetAsync($"{_apiGatewayUrl}/apigateway/orders");
        if (!response.IsSuccessStatusCode)
        {
            // Handle error
            ViewBag.Error = $"Failed to fetch orders: {response.StatusCode}";
            return View("Error");
        }

        var json = await response.Content.ReadAsStringAsync();
        var orders = JsonSerializer.Deserialize<List<OrderDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(orders);
        // return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
