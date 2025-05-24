
using OrderReportFunction.Models;

namespace OrderReportFunction.Services;
public interface IPdfGenerator
{
    byte[] GenerateUserReport(UserReport report);
}
