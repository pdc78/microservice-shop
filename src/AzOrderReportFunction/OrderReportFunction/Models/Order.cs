namespace OrderReportFunction.Models;

public class Order
{
    public required string OrderId { get; set; }
    public required string TransactionId { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal VAT { get; set; }
    public decimal TotalWithVAT => TotalPrice + VAT;
}
