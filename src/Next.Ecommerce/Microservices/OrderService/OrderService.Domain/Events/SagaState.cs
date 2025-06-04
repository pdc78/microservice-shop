namespace OrderService.Domain.Events;

// This class represents the state of a saga in the Order Service domain.
// It tracks the success and failure of various operations (inventory, payment, shipping) related to an order.
public class SagaState
{
    public Guid OrderId { get; set; }
    public bool InventorySuccess { get; set; }
    public bool PaymentSuccess { get; set; }
    public bool ShippingSuccess { get; set; }

    public bool InventoryFailed { get; set; }
    public bool PaymentFailed { get; set; }
    public bool ShippingFailed { get; set; }

    public bool IsCompleted => (InventorySuccess || InventoryFailed)
                             && (PaymentSuccess || PaymentFailed)
                             && (ShippingSuccess || ShippingFailed);
}
