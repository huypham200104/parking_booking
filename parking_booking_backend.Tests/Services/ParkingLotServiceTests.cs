using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class ParkingLotServiceTests
{
    [Fact]
    public async Task GetNearbyAsync_returns_active_lots_inside_radius_ordered_by_distance()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var near = TestData.ParkingLot(owner.Id);
        near.Name = "Near";
        var far = TestData.ParkingLot(owner.Id);
        far.Name = "Far";
        far.Location = new NetTopologySuite.Geometries.GeometryFactory(new NetTopologySuite.Geometries.PrecisionModel(), 4326)
            .CreatePoint(new NetTopologySuite.Geometries.Coordinate(106.7100, 10.7850));
        var suspended = TestData.ParkingLot(owner.Id);
        suspended.Name = "Suspended";
        suspended.Status = ParkingLotStatus.Suspended;

        await using (var context = database.CreateContext())
        {
            context.Users.Add(owner);
            context.ParkingLots.AddRange(near, far, suspended);
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var service = new ParkingLotService(queryContext, new TestCurrentUserService(Guid.NewGuid()));

        var result = await service.GetNearbyAsync(new NearbyParkingLotsQuery(10.7750, 106.7000, 10), CancellationToken.None);

        Assert.Equal(["Near", "Far"], result.Select(p => p.Name));
        Assert.All(result, parkingLot => Assert.Equal(ParkingLotStatus.Active, parkingLot.Status));
    }

    [Fact]
    public async Task GetByIdAsync_returns_detail_or_throws_when_missing()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var lot = TestData.ParkingLot(owner.Id);
        await using (var context = database.CreateContext())
        {
            context.Users.Add(owner);
            context.ParkingLots.Add(lot);
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var service = new ParkingLotService(queryContext, new TestCurrentUserService(Guid.NewGuid()));

        var result = await service.GetByIdAsync(lot.Id, CancellationToken.None);
        var missing = await Assert.ThrowsAsync<ApiException>(() => service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(lot.Id, result.Id);
        Assert.Equal(StatusCodes.Status404NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task ReportAsync_creates_report_when_inside_geofence_and_rejects_far_report()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var driver = TestData.User();
        var lot = TestData.ParkingLot(owner.Id);
        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(owner, driver);
            context.ParkingLots.Add(lot);
            await context.SaveChangesAsync();
        }

        await using var commandContext = database.CreateContext();
        var service = new ParkingLotService(commandContext, new TestCurrentUserService(driver.Id));

        await service.ReportAsync(lot.Id, new CrowdsourceReportRequest(ReportStatus.Available, 10.7750, 106.7000), CancellationToken.None);
        var farException = await Assert.ThrowsAsync<ApiException>(() =>
            service.ReportAsync(lot.Id, new CrowdsourceReportRequest(ReportStatus.Full, 500, 500), CancellationToken.None));

        Assert.Equal(StatusCodes.Status403Forbidden, farException.StatusCode);
        Assert.Equal(1, await commandContext.CrowdsourceReports.CountAsync());
    }

    [Fact]
    public async Task AddStaffAsync_adds_once_and_validates_owner_and_user()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var staff = TestData.User(role: Role.Guard, phone: "0900000002");
        var unrelated = TestData.User(phone: "0900000003");
        var lot = TestData.ParkingLot(owner.Id);
        await using (var context = database.CreateContext())
        {
            context.Users.AddRange(owner, staff, unrelated);
            context.ParkingLots.Add(lot);
            await context.SaveChangesAsync();
        }

        await using var ownerContext = database.CreateContext();
        var ownerService = new ParkingLotService(ownerContext, new TestCurrentUserService(owner.Id));
        await ownerService.AddStaffAsync(lot.Id, new AddParkingLotStaffRequest(staff.Id), CancellationToken.None);
        await ownerService.AddStaffAsync(lot.Id, new AddParkingLotStaffRequest(staff.Id), CancellationToken.None);

        await using var unrelatedContext = database.CreateContext();
        var unrelatedService = new ParkingLotService(unrelatedContext, new TestCurrentUserService(unrelated.Id));
        var forbidden = await Assert.ThrowsAsync<ApiException>(() =>
            unrelatedService.AddStaffAsync(lot.Id, new AddParkingLotStaffRequest(staff.Id), CancellationToken.None));
        var missingStaff = await Assert.ThrowsAsync<ApiException>(() =>
            ownerService.AddStaffAsync(lot.Id, new AddParkingLotStaffRequest(Guid.NewGuid()), CancellationToken.None));

        Assert.Equal(1, await ownerContext.ParkingLotStaffs.CountAsync());
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, missingStaff.StatusCode);
    }
}
