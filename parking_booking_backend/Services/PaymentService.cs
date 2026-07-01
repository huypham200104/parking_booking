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
        var descriptionParts = request.Data.Description
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        if (descriptionParts.Length >= 2
            && descriptionParts[0].Equals("WALLET", StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(descriptionParts[1], out var userId))
        {
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);
            if (wallet is null || request.Data.Amount <= 0)
            {
                return false;
            }

            var referenceId = string.IsNullOrWhiteSpace(request.Code)
                ? $"wallet-{userId}-{request.Data.TransactionDateTime}"
                : request.Code.Trim();
            if (await _dbContext.WalletTransactions.AnyAsync(item => item.ReferenceId == referenceId, cancellationToken))
            {
                return true;
            }

            wallet.Balance += request.Data.Amount;
            _dbContext.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = request.Data.Amount,
                Type = WalletTransactionType.Deposit,
                ReferenceId = referenceId
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }

        var bookingCode = descriptionParts.LastOrDefault();

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
