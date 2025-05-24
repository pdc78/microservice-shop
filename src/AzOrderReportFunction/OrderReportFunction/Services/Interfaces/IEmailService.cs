namespace OrderReportFunction.Services;

public interface IEmailService
{
    Task SendReportAsync(string toEmail, byte[] pdfBytes, string fileName);
}
