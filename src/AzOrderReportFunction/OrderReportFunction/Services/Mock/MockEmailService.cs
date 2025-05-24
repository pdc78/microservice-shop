using OrderReportFunction.Services;

public class MockEmailService : IEmailService
{
    public Task SendReportAsync(string toEmail, byte[] pdfBytes, string fileName)
    {
        Console.WriteLine($"[MockEmailService] Would send email to: {toEmail}, File: {fileName}, Size: {pdfBytes?.Length ?? 0} bytes");
        
        // Optionally, you could store the call info for assertions
        return Task.CompletedTask;
    }
}
