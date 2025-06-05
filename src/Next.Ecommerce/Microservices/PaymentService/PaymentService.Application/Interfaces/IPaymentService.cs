using PaymentService.Domain.Events;
using System.Threading.Tasks;

namespace PaymentService.Application.Interfaces;
public interface IPaymentService
{
    bool CancelPayment(PaymentCancelledEvent evt);
    Task SendPaymentConfirmedAsync(PaymentConfirmedEvent evt);
    Task SendPaymentRejectedAsync(PaymentFailedEvent evt);
}
