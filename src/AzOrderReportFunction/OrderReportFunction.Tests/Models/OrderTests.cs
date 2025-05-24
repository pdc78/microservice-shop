using OrderReportFunction.Models;

namespace OrderReportFunction.Tests.Models;

public class OrderTests
{
    [Test]
    public void TotalWithVAT_ShouldReturnCorrectSum()
    {
        // Arrange
        var order = new Order
        {
            OrderId = "ORD-123",
            TransactionId = "TXN-456",
            TotalPrice = 100m,
            VAT = 20m
        };

        // Act
        var total = order.TotalWithVAT;

        // Assert
        Assert.That(total, Is.EqualTo(120m));
    }

    [Test]
    public void Order_ShouldStoreRequiredPropertiesCorrectly()
    {
        // Arrange
        var orderId = "ORD-001";
        var transactionId = "TXN-001";

        // Act
        var order = new Order
        {
            OrderId = orderId,
            TransactionId = transactionId,
            TotalPrice = 250m,
            VAT = 50m
        };

        // Assert
        Assert.That(order.OrderId, Is.EqualTo(orderId));
        Assert.That(order.TransactionId, Is.EqualTo(transactionId));
        Assert.That(order.TotalPrice, Is.EqualTo(250m));
        Assert.That(order.VAT, Is.EqualTo(50m));
        Assert.That(order.TotalWithVAT, Is.EqualTo(300m));
    }

    [Test]
    public void TotalWithVAT_ShouldHandleZeroValues()
    {
        var order = new Order
        {
            OrderId = "ORD-ZERO",
            TransactionId = "TXN-ZERO",
            TotalPrice = 0m,
            VAT = 0m
        };

        Assert.That(order.TotalWithVAT, Is.EqualTo(0m));
    }

    [Test]
    public void TotalWithVAT_ShouldHandleNegativeValues()
    {
        var order = new Order
        {
            OrderId = "ORD-NEG",
            TransactionId = "TXN-NEG",
            TotalPrice = -100m,
            VAT = -20m
        };

        Assert.That(order.TotalWithVAT, Is.EqualTo(-120m));
    }

    [Test]
    public void TotalWithVAT_ShouldHandleLargeValues()
    {
        var order = new Order
        {
            OrderId = "ORD-BIG",
            TransactionId = "TXN-BIG",
            TotalPrice = 1_000_000m,
            VAT = 200_000m
        };

        Assert.That(order.TotalWithVAT, Is.EqualTo(1_200_000m));
    }

    [Test]
    public void TotalWithVAT_ShouldHandleHighPrecisionDecimals()
    {
        var order = new Order
        {
            OrderId = "ORD-PREC",
            TransactionId = "TXN-PREC",
            TotalPrice = 99.9999m,
            VAT = 0.0001m
        };

        Assert.That(order.TotalWithVAT, Is.EqualTo(100.0000m).Within(0.0001m));
    }
}
