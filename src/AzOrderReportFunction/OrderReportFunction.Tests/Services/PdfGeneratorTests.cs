using OrderReportFunction.Models;
using OrderReportFunction.Services;

namespace OrderReportFunction.Tests.Services;
public class PdfGeneratorTests
{
    private PdfGenerator _pdfGenerator;

    [SetUp]
    public void Setup()
    {
        _pdfGenerator = new PdfGenerator();
    }

    [Test]
    public void GenerateUserReport_WithValidData_ReturnsNonEmptyPdf()
    {
        var report = new UserReport
        {
            UserEmail = "test@example.com",
            Orders = new List<Order>
                {
                    new Order { OrderId = "123", TransactionId = "TXN456", TotalPrice = 100m, VAT = 22m }
                }
        };

        var result = _pdfGenerator.GenerateUserReport(report);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
        Assert.That(System.Text.Encoding.ASCII.GetString(result[..4]), Is.EqualTo("%PDF"));
    }

    [Test]
    public void GenerateUserReport_WithEmptyOrders_ReturnsValidPdf()
    {
        var report = new UserReport
        {
            UserEmail = "empty@example.com",
            Orders = new List<Order>()
        };

        var result = _pdfGenerator.GenerateUserReport(report);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
        Assert.That(System.Text.Encoding.ASCII.GetString(result[..4]), Is.EqualTo("%PDF"));
    }

    [Test]
    public void GenerateUserReport_WithNullEmail_ReturnsPdf()
    {
        var report = new UserReport
        {
            UserEmail = null,
            Orders = new List<Order>
                {
                    new Order { OrderId = "1", TransactionId = "T1", TotalPrice = 10, VAT = 2 }
                }
        };

        var result = _pdfGenerator.GenerateUserReport(report);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
        Assert.That(System.Text.Encoding.ASCII.GetString(result[..4]), Is.EqualTo("%PDF"));
    }

    [Test]
    public void GenerateUserReport_WithNullOrders_ThrowsException()
    {
        var report = new UserReport
        {
            UserEmail = "test@example.com",
            Orders = null
        };

        Assert.Throws<System.ArgumentNullException>(() =>
        {
            _pdfGenerator.GenerateUserReport(report);
        });
    }

    [Test]
    public void GenerateUserReport_WithLongEmailAndOrders_GeneratesPdf()
    {
        var report = new UserReport
        {
            UserEmail = new string('a', 250) + "@example.com",
            Orders = new List<Order>
                {
                    new Order { OrderId = new string('1', 100), TransactionId = "TX", TotalPrice = 0, VAT = 0 },
                    new Order { OrderId = "2", TransactionId = "TX2", TotalPrice = -10, VAT = -2 }
                }
        };

        var result = _pdfGenerator.GenerateUserReport(report);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
        Assert.That(System.Text.Encoding.ASCII.GetString(result[..4]), Is.EqualTo("%PDF"));
    }

    [Test]
    public void GenerateUserReport_WithZeroPriceAndVat_GeneratesPdf()
    {
        var report = new UserReport
        {
            UserEmail = "zero@example.com",
            Orders = new List<Order>
                {
                    new Order { OrderId = "0", TransactionId = "TX0", TotalPrice = 0, VAT = 0 }
                }
        };

        var result = _pdfGenerator.GenerateUserReport(report);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
        Assert.That(System.Text.Encoding.ASCII.GetString(result[..4]), Is.EqualTo("%PDF"));
    }
}