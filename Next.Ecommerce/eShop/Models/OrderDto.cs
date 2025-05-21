namespace eShop.Models;

public class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public List<string> Items { get; set; }
    public decimal TotalPrice { get; set; }
}