using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class NotificationAndFavouriteServiceTests
{
    [Fact]
    public async Task Notification_service_returns_owned_items_and_marks_them_read()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var user = TestData.User();
        await using var context = database.CreateContext();
        context.Users.Add(user);
        context.Notifications.AddRange(
            new Notification { UserId = user.Id, Title = "Mine", Message = "Message" },
            new Notification { UserId = Guid.NewGuid(), Title = "Other", Message = "Hidden" });
        await context.SaveChangesAsync();
        var service = new NotificationService(context, new TestCurrentUserService(user.Id));

        var items = await service.GetMineAsync(false, CancellationToken.None);
        await service.MarkReadAsync(items.Single().Id, CancellationToken.None);

        Assert.Single(items);
        Assert.True((await context.Notifications.SingleAsync(item => item.Id == items.Single().Id)).IsRead);
    }

    [Fact]
    public async Task Favourite_service_adds_lists_and_removes_active_parking_lot()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var user = TestData.User();
        var owner = TestData.User(role: Role.ParkingOwner);
        var lot = TestData.ParkingLot(owner.Id);
        await using var context = database.CreateContext();
        context.Users.AddRange(user, owner);
        context.ParkingLots.Add(lot);
        await context.SaveChangesAsync();
        var service = new FavouriteParkingLotService(context, new TestCurrentUserService(user.Id));

        await service.AddAsync(lot.Id, CancellationToken.None);
        await service.AddAsync(lot.Id, CancellationToken.None);
        var favourites = await service.GetMineAsync(CancellationToken.None);
        await service.RemoveAsync(lot.Id, CancellationToken.None);

        Assert.Single(favourites);
        Assert.Empty(await context.FavouriteParkingLots.ToListAsync());
    }
}
