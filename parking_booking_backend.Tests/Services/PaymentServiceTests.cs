using Microsoft.EntityFrameworkCore;
using parking_booking_backend.DTOs;
using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task ProcessWebhookAsync_credits_wallet_once()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var user = TestData.User();
        var wallet = TestData.Wallet(user.Id);
        await using (var context = database.CreateContext())
        {
            context.Users.Add(user);
            context.Wallets.Add(wallet);
            await context.SaveChangesAsync();
        }

        var request = new PaymentWebhookRequest("bank-transaction-1", new PaymentWebhookData(200_000, $"WALLET {user.Id}", DateTime.UtcNow.ToString("O")));
        await using (var firstContext = database.CreateContext())
        {
            Assert.True(await new PaymentService(firstContext).ProcessWebhookAsync(request, CancellationToken.None));
        }
        await using (var secondContext = database.CreateContext())
        {
            Assert.True(await new PaymentService(secondContext).ProcessWebhookAsync(request, CancellationToken.None));
        }

        await using var assertContext = database.CreateContext();
        Assert.Equal(200_000, (await assertContext.Wallets.SingleAsync()).Balance);
        Assert.Single(await assertContext.WalletTransactions.ToListAsync());
    }
}
