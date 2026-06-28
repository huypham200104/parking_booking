using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ProcessWebhookAsync(PaymentWebhookRequest request, CancellationToken cancellationToken)
    {
        var bookingCode = request.Data.Description
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault();

        if (string.IsNullOrWhiteSpace(bookingCode))
        {
            return false;
        }

        var transaction = await _dbContext.Transactions
            .Include(t => t.Booking)
            .FirstOrDefaultAsync(t => t.Booking != null
                && t.Booking.BookingCode == bookingCode
                && t.Status == TransactionStatus.Pending,
                cancellationToken);

        if (transaction is null)
        {
            return false;
        }

        transaction.Status = TransactionStatus.Success;
        transaction.TransactionDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
