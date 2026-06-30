using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class LayoutServiceTests
{
    [Fact]
    public async Task GetTemplatesAsync_returns_templates_ordered_by_name()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        await using (var context = database.CreateContext())
        {
            context.LayoutTemplates.AddRange(
                new LayoutTemplate { Name = "B", ImageUrl = "b", Description = "b" },
                new LayoutTemplate { Name = "A", ImageUrl = "a", Description = "a" });
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var service = new LayoutService(queryContext, new TestCurrentUserService(Guid.NewGuid()));

        var result = await service.GetTemplatesAsync(CancellationToken.None);

        Assert.Equal(["A", "B"], result.Select(t => t.Name));
    }

    [Fact]
    public async Task Floor_and_slot_methods_manage_layout_and_sync_capacity()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var lot = TestData.ParkingLot(owner.Id, availableSlots: 0);
        var template = TestData.LayoutTemplate();
        await using (var context = database.CreateContext())
        {
            context.Users.Add(owner);
            context.ParkingLots.Add(lot);
            context.LayoutTemplates.Add(template);
            await context.SaveChangesAsync();
        }

        await using var commandContext = database.CreateContext();
        var service = new LayoutService(commandContext, new TestCurrentUserService(owner.Id));

        var floor = await service.CreateFloorAsync(lot.Id, new CreateParkingFloorRequest(" B2 ", template.Id, null), CancellationToken.None);
        var floors = await service.GetFloorsAsync(lot.Id, CancellationToken.None);
        var slots = await service.SaveSlotsAsync(lot.Id, floor.Id,
        [
            new UpsertParkingSlotRequest(null, "A01", ParkingSlotStatus.Available, SlotVehicleType.Car, 10, 20, 60, 100, 0),
            new UpsertParkingSlotRequest(null, "A02", ParkingSlotStatus.Available, SlotVehicleType.Car, 80, 20, 60, 100, 0)
        ], CancellationToken.None);
        var fetchedSlots = await service.GetSlotsAsync(lot.Id, floor.Id, CancellationToken.None);

        Assert.Equal("B2", floor.FloorName);
        Assert.Single(floors);
        Assert.Equal(2, slots.Count);
        Assert.Equal(2, fetchedSlots.Count);

        await using var assertContext = database.CreateContext();
        var lotReloaded = await assertContext.ParkingLots.SingleAsync(p => p.Id == lot.Id);
        Assert.Equal(2, lotReloaded.TotalSlots);
        Assert.Equal(2, lotReloaded.AvailableSlots);
    }

    [Fact]
    public async Task CreateFloorAsync_allows_staff_and_rejects_unrelated_user_or_missing_template()
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
            context.ParkingLotStaffs.Add(new ParkingLotStaff { UserId = staff.Id, ParkingLotId = lot.Id });
            await context.SaveChangesAsync();
        }

        await using var staffContext = database.CreateContext();
        var staffService = new LayoutService(staffContext, new TestCurrentUserService(staff.Id));
        var floor = await staffService.CreateFloorAsync(lot.Id, new CreateParkingFloorRequest("Staff Floor", null, null), CancellationToken.None);

        await using var unrelatedContext = database.CreateContext();
        var unrelatedService = new LayoutService(unrelatedContext, new TestCurrentUserService(unrelated.Id));
        var forbidden = await Assert.ThrowsAsync<ApiException>(() =>
            unrelatedService.CreateFloorAsync(lot.Id, new CreateParkingFloorRequest("Denied", null, null), CancellationToken.None));

        await using var ownerContext = database.CreateContext();
        var ownerService = new LayoutService(ownerContext, new TestCurrentUserService(owner.Id));
        var missingTemplate = await Assert.ThrowsAsync<ApiException>(() =>
            ownerService.CreateFloorAsync(lot.Id, new CreateParkingFloorRequest("Missing", Guid.NewGuid(), null), CancellationToken.None));

        Assert.Equal("Staff Floor", floor.FloorName);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, missingTemplate.StatusCode);
    }

    [Fact]
    public async Task Get_and_save_slot_methods_throw_for_missing_floor_or_unknown_slot()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var lot = TestData.ParkingLot(owner.Id);
        var floor = TestData.Floor(lot.Id);
        await using (var context = database.CreateContext())
        {
            context.Users.Add(owner);
            context.ParkingLots.Add(lot);
            context.ParkingFloors.Add(floor);
            await context.SaveChangesAsync();
        }

        await using var commandContext = database.CreateContext();
        var service = new LayoutService(commandContext, new TestCurrentUserService(owner.Id));

        var missingFloor = await Assert.ThrowsAsync<ApiException>(() => service.GetSlotsAsync(lot.Id, Guid.NewGuid(), CancellationToken.None));
        var unknownSlot = await Assert.ThrowsAsync<ApiException>(() =>
            service.SaveSlotsAsync(lot.Id, floor.Id,
            [
                new UpsertParkingSlotRequest(Guid.NewGuid(), "A01", ParkingSlotStatus.Available, SlotVehicleType.Car, 10, 20, 60, 100, 0)
            ], CancellationToken.None));

        Assert.Equal(StatusCodes.Status404NotFound, missingFloor.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, unknownSlot.StatusCode);
    }
}
