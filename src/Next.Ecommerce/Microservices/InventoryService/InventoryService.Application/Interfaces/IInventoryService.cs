using InventoryService.Domain.Events;

namespace InventoryService.Application.Interfaces;
public interface IInventoryService
{
    bool ReserveInventory(InventoryReserveRequestEvent evt);
    Task SendInventoryConfirmedAsync(InventoryReservedConfirmedEvent evt);
    Task SendInventoryRejectedAsync(InventoryReservationFailedEvent evt);
}
