using OrderService.Domain.Events;

namespace InventoryService.Application.Interfaces;
public interface IInventoryService
{
    bool ReserveInventory(InventoryRequestedEvent evt);
    bool UnreserveInventory(InventoryCancelledEvent evt);
    Task SendInventoryConfirmedAsync(InventoryConfirmedEvent evt);
    Task SendInventoryRejectedAsync(InventoryCheckFailedEvent evt);
}
