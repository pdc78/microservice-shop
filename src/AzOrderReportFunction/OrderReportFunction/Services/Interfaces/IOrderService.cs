
using OrderReportFunction.Models;

namespace OrderReportFunction.Services;

public interface IOrderService
{
    Task<List<UserReport>> GetMonthlyUserReportsAsync(DateTime month);
}
