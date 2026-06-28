using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class WalletService : IWalletService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public WalletService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<WalletResponse> GetMineAsync(CancellationToken cancellationToken)
    {
        var wallet = await _dbContext.Wallets
            .AsNoTracking()
            .Include(w => w.WalletTransactions)
            .FirstOrDefaultAsync(w => w.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new ApiException("Wallet was not found.", StatusCodes.Status404NotFound);

        var transactions = wallet.WalletTransactions
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new WalletTransactionResponse(t.Id, t.Amount, t.Type, t.ReferenceId))
            .ToList();

        return new WalletResponse(wallet.Id, wallet.UserId, wallet.Balance, transactions);
    }

    public async Task<DepositResponse> CreateDepositAsync(DepositRequest request, CancellationToken cancellationToken)
    {
        var walletExists = await _dbContext.Wallets.AnyAsync(w => w.UserId == _currentUser.UserId, cancellationToken);
        if (!walletExists)
        {
            throw new ApiException("Wallet was not found.", StatusCodes.Status404NotFound);
        }

        await Task.CompletedTask;
        var vietQrUrl = $"https://img.vietqr.io/image/demo-bank-demo-account-compact2.png?amount={request.Amount:0}&addInfo=WALLET%20{_currentUser.UserId}";
        return new DepositResponse(request.Amount, vietQrUrl);
    }
}

