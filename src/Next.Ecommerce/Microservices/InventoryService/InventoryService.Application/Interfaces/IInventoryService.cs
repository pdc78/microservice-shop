using InventoryService.Domain.Events;

namespace InventoryService.Application.Interfaces;
public interface IInventoryService
{
    Task<bool> ReserveInventoryAsync(InventoryReserveEvent evt);
    Task SendInventoryConfirmedAsync(InventoryReservedConfirmedEvent evt);
    Task SendInventoryRejectedAsync(InventoryReservationFailedEvent evt);
}
