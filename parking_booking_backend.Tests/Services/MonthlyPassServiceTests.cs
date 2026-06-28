using Microsoft.AspNetCore.Http;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class MonthlyPassServiceTests
{
    [Fact]
    public async Task GetMineAsync_returns_only_active_passes_for_current_user()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var user = TestData.User();
        var otherUser = TestData.User(phone: "0900000002");
        var lot = TestData.ParkingLot(owner.Id);
        var vehicle = TestData.Vehicle(user.Id);
        var otherVehicle = TestData.Vehicle(otherUser.Id, "51F-999.99");
        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(owner, user, otherUser);
            context.ParkingLots.Add(lot);
            context.Vehicles.AddRange(vehicle, otherVehicle);
            context.MonthlyPasses.AddRange(
                new MonthlyPass { UserId = user.Id, VehicleId = vehicle.Id, ParkingLotId = lot.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(10), Price = 100_000, Status = MonthlyPassStatus.Active },
                new MonthlyPass { UserId = user.Id, VehicleId = vehicle.Id, ParkingLotId = lot.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(20), Price = 100_000, Status = MonthlyPassStatus.Expired },
                new MonthlyPass { UserId = otherUser.Id, VehicleId = otherVehicle.Id, ParkingLotId = lot.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(30), Price = 100_000, Status = MonthlyPassStatus.Active });
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var service = new MonthlyPassService(queryContext, new TestCurrentUserService(user.Id));

        var result = await service.GetMineAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(user.Id, result.Single().UserId);
        Assert.Equal(MonthlyPassStatus.Active, result.Single().Status);
    }

    [Fact]
    public async Task CreateAsync_creates_pass_from_owned_vehicle_and_lot_price()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var user = TestData.User();
        var vehicle = TestData.Vehicle(user.Id);
        var lot = TestData.ParkingLot(owner.Id, firstBlockPrice: 30_000);
        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(owner, user);
            context.Vehicles.Add(vehicle);
            context.ParkingLots.Add(lot);
            await context.SaveChangesAsync();
        }

        await using var commandContext = database.CreateContext();
        var service = new MonthlyPassService(commandContext, new TestCurrentUserService(user.Id));

        var result = await service.CreateAsync(new CreateMonthlyPassRequest(vehicle.Id, lot.Id, 30), CancellationToken.None);

        Assert.Equal(900_000, result.Price);
        Assert.Equal(MonthlyPassStatus.Active, result.Status);
    }

    [Fact]
    public async Task CreateAsync_throws_for_foreign_vehicle_or_missing_lot()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var user = TestData.User();
        var otherUser = TestData.User(phone: "0900000002");
        var otherVehicle = TestData.Vehicle(otherUser.Id);
        var lot = TestData.ParkingLot(owner.Id);
        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(owner, user, otherUser);
            context.Vehicles.Add(otherVehicle);
            context.ParkingLots.Add(lot);
            await context.SaveChangesAsync();
        }

        await using var commandContext = database.CreateContext();
        var service = new MonthlyPassService(commandContext, new TestCurrentUserService(user.Id));

        var vehicleException = await Assert.ThrowsAsync<ApiException>(() =>
            service.CreateAsync(new CreateMonthlyPassRequest(otherVehicle.Id, lot.Id, 30), CancellationToken.None));
        var lotException = await Assert.ThrowsAsync<ApiException>(() =>
            service.CreateAsync(new CreateMonthlyPassRequest(Guid.NewGuid(), Guid.NewGuid(), 30), CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, vehicleException.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, lotException.StatusCode);
    }
}
