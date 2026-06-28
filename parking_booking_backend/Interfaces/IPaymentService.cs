using parking_booking_backend.DTOs;

namespace parking_booking_backend.Interfaces;

public interface IPaymentService
{
    Task<bool> ProcessWebhookAsync(PaymentWebhookRequest request, CancellationToken cancellationToken);
}
