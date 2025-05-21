using eShop.Models;

namespace eShop.ViewModels;

public class ProductCatalogViewModel
{
    public List<ProductDto> Products { get; set; } = new();
    public BasketDto? Basket { get; set; }
}
