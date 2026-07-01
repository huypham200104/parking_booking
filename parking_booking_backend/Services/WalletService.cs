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

    public async Task<AdminWalletStatsResponse> GetAdminStatsAsync(CancellationToken cancellationToken)
    {
        var deposits = _dbContext.WalletTransactions
            .AsNoTracking()
            .Where(transaction => transaction.Type == WalletTransactionType.Deposit
                && transaction.Wallet != null
                && transaction.Wallet.User != null
                && transaction.Wallet.User.Role == Role.Driver);
        var today = DateTime.UtcNow.Date;

        var totalDeposited = await deposits.SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        var depositedToday = await deposits
            .Where(transaction => transaction.CreatedAt >= today)
            .SumAsync(transaction => (decimal?)transaction.Amount, cancellationToken) ?? 0;
        var depositCount = await deposits.CountAsync(cancellationToken);
        var recentDeposits = await deposits
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Take(8)
            .Select(transaction => new AdminWalletDepositResponse(
                transaction.Id,
                transaction.Wallet!.User!.PhoneNumber,
                transaction.Wallet.User.FullName,
                transaction.Amount,
                transaction.CreatedAt))
            .ToListAsync(cancellationToken);

        return new AdminWalletStatsResponse(totalDeposited, depositedToday, depositCount, recentDeposits);
    }

    public async Task<PaginationResponse<AdminUserWalletResponse>> GetAdminUserWalletsAsync(int pageIndex, int pageSize, string? keyword, CancellationToken cancellationToken)
    {
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _dbContext.Wallets.AsNoTracking()
            .Where(wallet => wallet.User != null && wallet.User.Role == Role.Driver);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(wallet => wallet.User!.FullName.Contains(normalizedKeyword) || wallet.User.PhoneNumber.Contains(normalizedKeyword));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(wallet => wallet.User!.FullName)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(wallet => new AdminUserWalletResponse(
                wallet.UserId,
                wallet.User!.PhoneNumber,
                wallet.User.FullName,
                wallet.WalletTransactions.Where(transaction => transaction.Type == WalletTransactionType.Deposit).Sum(transaction => (decimal?)transaction.Amount) ?? 0,
                wallet.Balance))
            .ToListAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginationResponse<AdminUserWalletResponse>(items, totalCount, pageIndex, pageSize, totalPages);
    }
}
