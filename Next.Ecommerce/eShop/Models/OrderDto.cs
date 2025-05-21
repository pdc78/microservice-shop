using System.ComponentModel.DataAnnotations;

namespace eShop.Models;

public class OrderDto
{
    public int Id { get; set; }
    [Required]
    public string CustomerName { get; set; }
    [Required]
    public List<string> Items { get; set; }
    public decimal TotalPrice { get; set; }
}