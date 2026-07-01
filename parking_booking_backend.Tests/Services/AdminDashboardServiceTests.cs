using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class AdminDashboardServiceTests
{
    [Fact]
    public async Task GetAsync_returns_real_counts_recent_bookings_and_uses_vietnam_day_boundary()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var now = new DateTimeOffset(2026, 7, 1, 1, 0, 0, TimeSpan.Zero);
        var owner = TestData.User(role: Role.ParkingOwner);
        owner.CreatedAt = new DateTime(2026, 6, 29, 0, 0, 0, DateTimeKind.Utc);
        var todayDriver = TestData.User();
        todayDriver.CreatedAt = new DateTime(2026, 6, 30, 17, 0, 0, DateTimeKind.Utc);
        var previousDriver = TestData.User();
        previousDriver.CreatedAt = todayDriver.CreatedAt.AddTicks(-1);
        var vehicle = TestData.Vehicle(todayDriver.Id);
        var activeLot = TestData.ParkingLot(owner.Id);
        var pendingLot = TestData.ParkingLot(owner.Id);
        pendingLot.Status = ParkingLotStatus.PendingApproval;
        var floor = TestData.Floor(activeLot.Id);
        var slot = TestData.Slot(floor.Id);
        var todayBooking = TestData.Booking(todayDriver.Id, vehicle.Id, activeLot.Id, slot.Id, BookingStatus.Pending);
        todayBooking.BookingTimestamp = todayDriver.CreatedAt;
        var previousBooking = TestData.Booking(todayDriver.Id, vehicle.Id, activeLot.Id, slot.Id, BookingStatus.Completed);
        previousBooking.BookingTimestamp = todayDriver.CreatedAt.AddTicks(-1);

        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(owner, todayDriver, previousDriver);
            context.Vehicles.Add(vehicle);
            context.ParkingLots.AddRange(activeLot, pendingLot);
            context.ParkingFloors.Add(floor);
            context.ParkingSlots.Add(slot);
            context.Bookings.AddRange(todayBooking, previousBooking);
            await context.SaveChangesAsync();
            owner.CreatedAt = new DateTime(2026, 6, 29, 0, 0, 0, DateTimeKind.Utc);
            todayDriver.CreatedAt = new DateTime(2026, 6, 30, 17, 0, 0, DateTimeKind.Utc);
            previousDriver.CreatedAt = todayDriver.CreatedAt.AddTicks(-1);
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var result = await new AdminDashboardService(queryContext, new FixedTimeProvider(now)).GetAsync(CancellationToken.None);

        Assert.Equal(1, result.BookingsToday);
        Assert.Equal(1, result.ActiveParkingLots);
        Assert.Equal(2, result.TotalParkingLots);
        Assert.Equal(1, result.PendingParkingLots);
        Assert.Equal(1, result.NewUsersToday);
        Assert.Equal(todayBooking.Id, result.RecentBookings.First().Id);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
