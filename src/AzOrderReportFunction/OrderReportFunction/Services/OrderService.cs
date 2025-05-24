using Microsoft.Extensions.Configuration;
using OrderReportFunction.Models;

namespace OrderReportFunction.Services;

public class OrderService : IOrderService
{
    private readonly string _connectionStrings;

    public OrderService(IConfiguration config)
    {
        _connectionStrings = config["ConnectionStrings:OrdersDb"] ?? throw new ArgumentNullException(nameof(config), "SendGridApiKey is missing in configuration.");
    }

    public async Task<List<UserReport>> GetMonthlyUserReportsAsync(DateTime month)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("Replace with real DB logic");
    }
}