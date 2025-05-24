using OrderReportFunction.Models;
using OrderReportFunction.Services;

namespace OrderReportFunction.Mock.Services;

public class MockOrderService : IOrderService
{
    public async Task<List<UserReport>> GetMonthlyUserReportsAsync(DateTime month)
    {
        await Task.Delay(100); // Simulate async

        return new List<UserReport>
        {
            new UserReport
            {
                UserEmail = "test1@test.com",
                Orders = new List<Order>
                {
                    new Order { OrderId = "MOCK-001", TotalPrice = 100, VAT = 25, TransactionId = "T-001"},
                    new Order { OrderId = "MOCK-002", TotalPrice = 200, VAT = 50, TransactionId = "T-002" }
                }
            },
            new UserReport
            {
                UserEmail = "test2@test.com",
                Orders = new List<Order>
                {
                    new Order { OrderId = "MOCK-001", TotalPrice = 100, VAT = 25 , TransactionId = "T-001"},
                    new Order { OrderId = "MOCK-002", TotalPrice = 200, VAT = 50 , TransactionId = "T-002" }
                }
            }
        };
    }
}
