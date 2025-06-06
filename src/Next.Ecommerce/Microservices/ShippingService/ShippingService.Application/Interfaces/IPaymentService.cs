using ShippingService.Domain.Events;
using System.Threading.Tasks;

namespace ShippingService.Application.Interfaces;
public interface IShippingService
{
    bool CancelShipping(ShippingCancelledEvent evt);
    Task SendShippingConfirmedAsync(ShippingConfirmedEvent evt);
    Task SendShippingRejectedAsync(ShippingFailedEvent evt);
}
