using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data.Seed;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Data;

public sealed class MockDataSeederTests
{
    [Fact]
    public async Task SeedAsync_creates_expected_expanded_dataset_and_is_idempotent()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        await using var context = database.CreateContext();
        var seeder = new MockDataSeeder(context);

        var first = await seeder.SeedAsync(false, CancellationToken.None);
        var second = await seeder.SeedAsync(false, CancellationToken.None);

        Assert.True(first.Created);
        Assert.Equal(28, first.Users);
        Assert.True(first.ParkingLots > 0);
        Assert.True(first.ParkingFloors > 0);
        Assert.True(first.ParkingSlots > 0);
        Assert.Equal(16, first.Vehicles);
        Assert.Equal(6, first.Vouchers);
        Assert.False(second.Created);

        Assert.Equal(28, await context.Users.CountAsync());
        Assert.Equal(first.ParkingLots, await context.ParkingLots.CountAsync());
        Assert.Equal(first.ParkingFloors, await context.ParkingFloors.CountAsync());
        Assert.Equal(first.ParkingSlots, await context.ParkingSlots.CountAsync());
        Assert.Equal(81, await context.Notifications.CountAsync());
        Assert.True(await context.Notifications.AnyAsync(notification => notification.IsRead));
        Assert.True(await context.Notifications.AnyAsync(notification => !notification.IsRead));
    }
}
