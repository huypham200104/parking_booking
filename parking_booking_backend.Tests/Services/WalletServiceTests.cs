using Microsoft.AspNetCore.Http;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class WalletServiceTests
{
    [Fact]
    public async Task GetMineAsync_returns_wallet_with_transactions_descending()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var user = TestData.User();
        var wallet = TestData.Wallet(user.Id, 250_000);
        await using (var context = database.CreateContext())
        {
            context.Users.Add(user);
            context.Wallets.Add(wallet);
            context.WalletTransactions.AddRange(
                new WalletTransaction { WalletId = wallet.Id, Amount = 50_000, Type = WalletTransactionType.Deposit, ReferenceId = "old", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new WalletTransaction { WalletId = wallet.Id, Amount = -20_000, Type = WalletTransactionType.Payment, ReferenceId = "new", CreatedAt = DateTime.UtcNow.AddDays(-1) });
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var service = new WalletService(queryContext, new TestCurrentUserService(user.Id));

        var result = await service.GetMineAsync(CancellationToken.None);

        Assert.Equal(250_000, result.Balance);
        Assert.Equal(["new", "old"], result.Transactions.Select(t => t.ReferenceId));
    }

    [Fact]
    public async Task CreateDepositAsync_returns_vietqr_url_when_wallet_exists()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var user = TestData.User();
        await using (var context = database.CreateContext())
        {
            context.Users.Add(user);
            context.Wallets.Add(TestData.Wallet(user.Id));
            await context.SaveChangesAsync();
        }

        await using var commandContext = database.CreateContext();
        var service = new WalletService(commandContext, new TestCurrentUserService(user.Id));

        var result = await service.CreateDepositAsync(new DepositRequest(200_000), CancellationToken.None);

        Assert.Equal(200_000, result.Amount);
        Assert.Contains("amount=200000", result.VietQrUrl, StringComparison.Ordinal);
        Assert.Contains(user.Id.ToString(), result.VietQrUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Wallet_methods_throw_when_wallet_missing()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        await using var context = database.CreateContext();
        var service = new WalletService(context, new TestCurrentUserService(Guid.NewGuid()));

        var getException = await Assert.ThrowsAsync<ApiException>(() => service.GetMineAsync(CancellationToken.None));
        var depositException = await Assert.ThrowsAsync<ApiException>(() => service.CreateDepositAsync(new DepositRequest(10_000), CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, getException.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, depositException.StatusCode);
    }

    [Fact]
    public async Task GetAdminStatsAsync_sums_deposits_and_ignores_payments()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var user = TestData.User();
        var wallet = TestData.Wallet(user.Id);
        var owner = TestData.User(role: Role.ParkingOwner);
        var ownerWallet = TestData.Wallet(owner.Id, 500_000);
        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(user, owner);
            context.Wallets.AddRange(wallet, ownerWallet);
            context.WalletTransactions.AddRange(
                new WalletTransaction { WalletId = wallet.Id, Amount = 150_000, Type = WalletTransactionType.Deposit, ReferenceId = "deposit" },
                new WalletTransaction { WalletId = wallet.Id, Amount = -20_000, Type = WalletTransactionType.Payment, ReferenceId = "payment" },
                new WalletTransaction { WalletId = ownerWallet.Id, Amount = 500_000, Type = WalletTransactionType.Deposit, ReferenceId = "owner-deposit" });
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var service = new WalletService(queryContext, new TestCurrentUserService(Guid.NewGuid()));
        var result = await service.GetAdminStatsAsync(CancellationToken.None);

        Assert.Equal(150_000, result.TotalDeposited);
        Assert.Equal(150_000, result.DepositedToday);
        Assert.Equal(1, result.DepositCount);
        Assert.Equal(user.PhoneNumber, Assert.Single(result.RecentDeposits).PhoneNumber);

        var userWallets = await service.GetAdminUserWalletsAsync(1, 10, user.PhoneNumber, CancellationToken.None);
        var userWallet = Assert.Single(userWallets.Items);
        Assert.Equal(150_000, userWallet.TotalDeposited);
        Assert.Equal(wallet.Balance, userWallet.Balance);
        Assert.DoesNotContain(userWallets.Items, item => item.UserId == owner.Id);
    }
}
