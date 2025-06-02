namespace OrderService.Domain.Events;

public class InventoryReserveRequestEvent
{
    public Guid OrderId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

public class InventoryReservedConfirmedEvent
{
    public Guid OrderId { get; set; }
}

public class InventoryReservationFailedEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
}