using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using eShop.Models;
using eShop.Services.Interfaces;

namespace eShop.Controllers;

public class HomeController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetOrdersAsync();
        return View(orders);
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
