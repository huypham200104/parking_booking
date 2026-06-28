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
        Assert.Equal(24, first.Users);
        Assert.Equal(15, first.ParkingLots);
        Assert.Equal(18, first.ParkingFloors);
        Assert.Equal(192, first.ParkingSlots);
        Assert.Equal(12, first.Vehicles);
        Assert.Equal(6, first.Vouchers);
        Assert.False(second.Created);

        Assert.Equal(24, await context.Users.CountAsync());
        Assert.Equal(15, await context.ParkingLots.CountAsync());
        Assert.Equal(18, await context.ParkingFloors.CountAsync());
        Assert.Equal(192, await context.ParkingSlots.CountAsync());
    }
}
