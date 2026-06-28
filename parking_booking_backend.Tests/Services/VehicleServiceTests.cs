using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class VehicleServiceTests
{
    [Fact]
    public async Task GetMineAsync_returns_only_current_user_vehicles_ordered_by_default()
    {
        await using var database = await TestDatabase.CreateSqliteAsync();
        var currentUser = TestData.User();
        var otherUser = TestData.User(phone: "0900000002");
        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(currentUser, otherUser);
            context.Vehicles.AddRange(
                TestData.Vehicle(currentUser.Id, "51F-222.22", false),
                TestData.Vehicle(currentUser.Id, "51F-111.11", true),
                TestData.Vehicle(otherUser.Id, "51F-333.33", true));
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var service = new VehicleService(queryContext, new TestCurrentUserService(currentUser.Id));

        var result = await service.GetMineAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.True(result.First().IsDefault);
        Assert.All(result, vehicle => Assert.Equal(currentUser.Id, vehicle.UserId));
    }

    [Fact]
    public async Task CreateAsync_normalizes_plate_and_clears_previous_default()
    {
        await using var database = await TestDatabase.CreateSqliteAsync();
        var user = TestData.User();
        var oldDefault = TestData.Vehicle(user.Id, "51F-111.11", true);
        await using (var context = database.CreateContext())
        {
            context.Users.Add(user);
            context.Vehicles.Add(oldDefault);
            await context.SaveChangesAsync();
        }

        await using var commandContext = database.CreateContext();
        var service = new VehicleService(commandContext, new TestCurrentUserService(user.Id));

        var result = await service.CreateAsync(new CreateVehicleRequest(" 51g-999.99 ", VehicleType.SUV, true), CancellationToken.None);

        Assert.Equal("51G-999.99", result.LicensePlate);
        Assert.True(result.IsDefault);

        await using var assertContext = database.CreateContext();
        var oldDefaultReloaded = await assertContext.Vehicles.SingleAsync(v => v.Id == oldDefault.Id);
        Assert.False(oldDefaultReloaded.IsDefault);
    }

    [Fact]
    public async Task CreateAsync_throws_for_duplicate_plate()
    {
        await using var database = await TestDatabase.CreateSqliteAsync();
        var user = TestData.User();
        await using (var context = database.CreateContext())
        {
            context.Users.Add(user);
            context.Vehicles.Add(TestData.Vehicle(user.Id, "51F-123.45"));
            await context.SaveChangesAsync();
        }

        await using var commandContext = database.CreateContext();
        var service = new VehicleService(commandContext, new TestCurrentUserService(user.Id));

        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            service.CreateAsync(new CreateVehicleRequest("51F-123.45", VehicleType.Sedan, false), CancellationToken.None));
        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
    }
}

