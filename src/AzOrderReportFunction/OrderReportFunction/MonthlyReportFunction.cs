using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using OrderReportFunction.Models;
using OrderReportFunction.Services;

namespace OrderReportFunction
{
    public class MonthlyReportFunction
    {
        private readonly ILogger<MonthlyReportFunction> _logger;
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IOrderService _orderService;
        private readonly IEmailService _emailService;

        public MonthlyReportFunction(ILogger<MonthlyReportFunction> logger, IPdfGenerator pdfGenerator, IOrderService orderService, IEmailService emailService)
        {
            _logger = logger;
            _orderService = orderService;
            _pdfGenerator = pdfGenerator;
            _emailService = emailService;
        }


        [Function("MonthlyReportFunction")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo)
        {
            _logger.LogInformation("Timer triggered at {Time}", DateTime.Now);

            LogNextSchedule(timerInfo);

            if (!IsLastDayOfMonth(DateTime.Today))
            {
                _logger.LogInformation("Not the last day of the month. Skipping report generation.");
                return;
            }

            _logger.LogInformation("It's the last day of the month. Starting report generation...");

            try
            {
                var reports = await _orderService.GetMonthlyUserReportsAsync(DateTime.Today);

                foreach (var report in reports)
                {
                    await ProcessReportAsync(report);
                }

                _logger.LogInformation("All reports processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during report generation or sending.");
            }
        }

        private void LogNextSchedule(TimerInfo timerInfo)
        {
            if (timerInfo.ScheduleStatus is not null)
            {
                _logger.LogInformation("Next execution scheduled at {NextRun}", timerInfo.ScheduleStatus.Next);
            }
        }

        private bool IsLastDayOfMonth(DateTime date)
        {
            return date.Month != date.AddDays(1).Month;
        }

        private async Task ProcessReportAsync(UserReport report)
        {
            var pdf = _pdfGenerator.GenerateUserReport(report);

            if (pdf is null || pdf.Length == 0)
            {
                _logger.LogWarning("PDF generation failed for {User}", report.UserEmail);
                return;
            }

            var fileName = $"MonthlyReport_{DateTime.Today:yyyy_MM}_{report.UserEmail.Replace("@", "_at_")}.pdf";

            await _emailService.SendReportAsync(report.UserEmail, pdf, fileName);

            _logger.LogInformation("Report sent to {User}", report.UserEmail);
        }
    }
}
