// using InventoryService.Application;
// using InventoryService.Application.Entities;
// using InventoryService.Domain.Entities;

// namespace InventoryService.Infrastructure.Repositories;

// public class InMemoryInventoryRepository : IInventoryRepository
// {
//     private readonly List<InventoryItem> _items = new()
//     {
//         new InventoryItem { ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"), AvailableQuantity = 10 },
//         new InventoryItem { ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"), AvailableQuantity = 5 },
//     };

//     public Task<bool> CheckAvailabilityAsync(Guid productId, int quantity)
//     {
//         var item = _items.FirstOrDefault(i => i.ProductId == productId);
//         return Task.FromResult(item != null && item.AvailableQuantity >= quantity);
//     }
// }
