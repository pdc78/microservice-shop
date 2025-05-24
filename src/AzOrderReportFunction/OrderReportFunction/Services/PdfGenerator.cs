using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using OrderReportFunction.Models;

namespace OrderReportFunction.Services;

public class PdfGenerator : IPdfGenerator
{
    public byte[] GenerateUserReport(UserReport report)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        if (report.Orders == null)
            throw new ArgumentNullException(nameof(report.Orders));

        using var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        var font = new XFont("Verdana", 12, XFontStyle.Regular);

        double y = 40;

        gfx.DrawString($"Monthly Report for {report.UserEmail}", font, XBrushes.Black,
            new XRect(0, y, page.Width, page.Height), XStringFormats.TopCenter);

        y += 40;

        foreach (var order in report.Orders)
        {
            var line = $"Order ID: {order.OrderId}  | TransactionId: {order.TransactionId:C} | Total: {order.TotalPrice:C} | VAT: {order.VAT:C} | Total with VAT: {order.TotalWithVAT:C}";
            gfx.DrawString(line, font, XBrushes.Black, new XRect(40, y, page.Width - 80, page.Height), XStringFormats.TopLeft);
            y += 25;
        }

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }
}
