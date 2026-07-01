using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class BookingServiceTests
{
    [Fact]
    public async Task CreateAsync_creates_booking_and_occupies_slot()
    {
        var fixture = await CreateBookingFixtureAsync();
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var result = await service.CreateAsync(new CreateBookingRequest(fixture.Slot.Id, fixture.Vehicle.Id, null), CancellationToken.None);
        var historyItem = Assert.Single(await service.GetMineAsync(CancellationToken.None));

        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.Equal(fixture.Driver.Id, result.UserId);
        Assert.Equal(fixture.ParkingLot.Id, result.ParkingLotId);
        Assert.Equal(6, result.BookingCode.Length);
        Assert.Equal(BookingPolicy.GetCheckInDeadline(result.BookingTimestamp), result.CheckInDeadline);
        Assert.Equal(result.CheckInDeadline, historyItem.CheckInDeadline);

        var slot = await context.ParkingSlots.SingleAsync(s => s.Id == fixture.Slot.Id);
        var lot = await context.ParkingLots.SingleAsync(p => p.Id == fixture.ParkingLot.Id);
        Assert.Equal(ParkingSlotStatus.Occupied, slot.Status);
        Assert.Equal(0, lot.AvailableSlots);
    }

    [Fact]
    public async Task CreateAsync_supports_guest_license_plate()
    {
        var fixture = await CreateBookingFixtureAsync();
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var result = await service.CreateAsync(new CreateBookingRequest(fixture.Slot.Id, null, " 51f-999.99 "), CancellationToken.None);

        Assert.Null(result.UserId);
        Assert.Equal("51F-999.99", result.GuestLicensePlate);
    }

    [Fact]
    public async Task CreateAsync_rejects_missing_vehicle_and_unavailable_slot()
    {
        var fixture = await CreateBookingFixtureAsync(slotStatus: ParkingSlotStatus.Occupied);
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var missingVehicle = await Assert.ThrowsAsync<ApiException>(() =>
            service.CreateAsync(new CreateBookingRequest(fixture.Slot.Id, null, null), CancellationToken.None));
        var unavailable = await Assert.ThrowsAsync<ApiException>(() =>
            service.CreateAsync(new CreateBookingRequest(fixture.Slot.Id, fixture.Vehicle.Id, null), CancellationToken.None));

        Assert.Equal(StatusCodes.Status400BadRequest, missingVehicle.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, unavailable.StatusCode);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.CheckedIn)]
    public async Task CreateAsync_rejects_when_driver_has_active_booking(BookingStatus status)
    {
        var fixture = await CreateBookingFixtureAsync();
        var activeBooking = await AddBookingAsync(fixture, status);
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            service.CreateAsync(new CreateBookingRequest(fixture.Slot.Id, fixture.Vehicle.Id, null), CancellationToken.None));

        Assert.Equal(StatusCodes.Status409Conflict, exception.StatusCode);
        Assert.Equal("ACTIVE_BOOKING_EXISTS", exception.ErrorCode);
        Assert.Equal(activeBooking.Id, exception.ActiveBookingId);
    }

    [Theory]
    [InlineData(BookingStatus.Completed)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.NoShow)]
    public async Task CreateAsync_allows_new_booking_after_closed_booking(BookingStatus status)
    {
        var fixture = await CreateBookingFixtureAsync();
        await AddBookingAsync(fixture, status);
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var result = await service.CreateAsync(new CreateBookingRequest(fixture.Slot.Id, fixture.Vehicle.Id, null), CancellationToken.None);

        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CheckInAsync_updates_pending_booking_and_rejects_invalid_access_or_status()
    {
        var fixture = await CreateBookingFixtureAsync();
        var booking = await AddBookingAsync(fixture, BookingStatus.Pending);
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var result = await service.CheckInAsync(booking.Id, CancellationToken.None);
        var invalidStatus = await Assert.ThrowsAsync<ApiException>(() => service.CheckInAsync(booking.Id, CancellationToken.None));

        await using var otherContext = fixture.Database.CreateContext();
        var otherService = new BookingService(otherContext, new TestCurrentUserService(Guid.NewGuid()));
        var forbidden = await Assert.ThrowsAsync<ApiException>(() => otherService.CheckInAsync(booking.Id, CancellationToken.None));

        Assert.Equal(BookingStatus.CheckedIn, result.Status);
        Assert.NotNull(result.CheckInTimestamp);
        Assert.Equal(StatusCodes.Status409Conflict, invalidStatus.StatusCode);
        Assert.Equal(StatusCodes.Status403Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task ApplyVoucherAsync_sets_valid_voucher_and_rejects_closed_or_missing_voucher()
    {
        var fixture = await CreateBookingFixtureAsync();
        var booking = await AddBookingAsync(fixture, BookingStatus.Pending);
        await using (var seedContext = fixture.Database.CreateContext())
        {
            seedContext.Vouchers.Add(TestData.Voucher("FREE10K"));
            seedContext.Vouchers.Add(new Voucher { Code = "EXPIRED", DiscountAmount = 10_000, ExpiryDate = DateTime.UtcNow.AddDays(-1), UsageLimit = 10 });
            await seedContext.SaveChangesAsync();
        }

        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var result = await service.ApplyVoucherAsync(booking.Id, new ApplyVoucherRequest(" free10k "), CancellationToken.None);
        var missing = await Assert.ThrowsAsync<ApiException>(() =>
            service.ApplyVoucherAsync(booking.Id, new ApplyVoucherRequest("NONE"), CancellationToken.None));
        var expired = await Assert.ThrowsAsync<ApiException>(() =>
            service.ApplyVoucherAsync(booking.Id, new ApplyVoucherRequest("EXPIRED"), CancellationToken.None));

        var closed = await AddBookingAsync(fixture, BookingStatus.Completed, "DONE01");
        var closedException = await Assert.ThrowsAsync<ApiException>(() =>
            service.ApplyVoucherAsync(closed.Id, new ApplyVoucherRequest("FREE10K"), CancellationToken.None));

        Assert.NotNull(result);
        Assert.Equal(StatusCodes.Status404NotFound, missing.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, expired.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, closedException.StatusCode);
    }

    [Fact]
    public async Task CheckOutAsync_with_wallet_completes_booking_creates_payment_and_releases_slot()
    {
        var fixture = await CreateBookingFixtureAsync();
        await using (var seedContext = fixture.Database.CreateContext())
        {
            seedContext.Wallets.Add(TestData.Wallet(fixture.Driver.Id, 200_000));
            await seedContext.SaveChangesAsync();
        }
        var booking = await AddBookingAsync(fixture, BookingStatus.CheckedIn);

        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var result = await service.CheckOutAsync(booking.Id, new CheckOutRequest(true), CancellationToken.None);

        Assert.Equal(TransactionStatus.Success, result.Status);
        Assert.Null(result.VietQrUrl);

        var bookingReloaded = await context.Bookings.SingleAsync(b => b.Id == booking.Id);
        var slotReloaded = await context.ParkingSlots.SingleAsync(s => s.Id == fixture.Slot.Id);
        var wallet = await context.Wallets.Include(w => w.WalletTransactions).SingleAsync(w => w.UserId == fixture.Driver.Id);
        Assert.Equal(BookingStatus.Completed, bookingReloaded.Status);
        Assert.Equal(ParkingSlotStatus.Available, slotReloaded.Status);
        Assert.Single(wallet.WalletTransactions);
        Assert.True(wallet.Balance < 200_000);
    }

    [Fact]
    public async Task CheckOutAsync_without_wallet_returns_pending_vietqr_transaction()
    {
        var fixture = await CreateBookingFixtureAsync();
        var booking = await AddBookingAsync(fixture, BookingStatus.CheckedIn);
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var result = await service.CheckOutAsync(booking.Id, new CheckOutRequest(false), CancellationToken.None);

        Assert.Equal(TransactionStatus.Pending, result.Status);
        Assert.Contains("PKB", result.VietQrUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task CheckOutAsync_rejects_non_checked_in_booking_or_insufficient_wallet()
    {
        var fixture = await CreateBookingFixtureAsync();
        await using (var seedContext = fixture.Database.CreateContext())
        {
            seedContext.Wallets.Add(TestData.Wallet(fixture.Driver.Id, 1));
            await seedContext.SaveChangesAsync();
        }
        var checkedIn = await AddBookingAsync(fixture, BookingStatus.CheckedIn);
        var pending = await AddBookingAsync(fixture, BookingStatus.Pending, "PEND01");

        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        var insufficient = await Assert.ThrowsAsync<ApiException>(() =>
            service.CheckOutAsync(checkedIn.Id, new CheckOutRequest(true), CancellationToken.None));
        var invalidStatus = await Assert.ThrowsAsync<ApiException>(() =>
            service.CheckOutAsync(pending.Id, new CheckOutRequest(false), CancellationToken.None));

        Assert.Equal(StatusCodes.Status409Conflict, insufficient.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, invalidStatus.StatusCode);
    }

    [Fact]
    public async Task CancelAsync_cancels_pending_booking_and_releases_slot()
    {
        var fixture = await CreateBookingFixtureAsync();
        var booking = await AddBookingAsync(fixture, BookingStatus.Pending);
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Driver.Id));

        await service.CancelAsync(booking.Id, CancellationToken.None);
        var secondCancel = await Assert.ThrowsAsync<ApiException>(() => service.CancelAsync(booking.Id, CancellationToken.None));

        var bookingReloaded = await context.Bookings.SingleAsync(b => b.Id == booking.Id);
        var slotReloaded = await context.ParkingSlots.SingleAsync(s => s.Id == fixture.Slot.Id);

        Assert.Equal(BookingStatus.Cancelled, bookingReloaded.Status);
        Assert.Equal(ParkingSlotStatus.Available, slotReloaded.Status);
        Assert.Equal(StatusCodes.Status409Conflict, secondCancel.StatusCode);
    }

    [Fact]
    public async Task ProcessQrAsync_checks_in_pending_booking_for_assigned_staff()
    {
        var fixture = await CreateBookingFixtureAsync();
        var booking = await AddBookingAsync(fixture, BookingStatus.Pending);
        var staff = TestData.User(role: Role.Guard);
        await using (var seedContext = fixture.Database.CreateContext())
        {
            seedContext.Users.Add(staff);
            seedContext.ParkingLotStaffs.Add(new ParkingLotStaff { UserId = staff.Id, ParkingLotId = fixture.ParkingLot.Id });
            await seedContext.SaveChangesAsync();
        }
        string token;
        await using (var driverContext = fixture.Database.CreateContext())
        {
            token = (await new BookingService(driverContext, new TestCurrentUserService(fixture.Driver.Id))
                .GenerateQrTokenAsync(booking.Id, CancellationToken.None)).QrToken;
        }
        await using var staffContext = fixture.Database.CreateContext();
        var result = await new BookingService(staffContext, new TestCurrentUserService(staff.Id))
            .ProcessQrAsync(new VerifyQrRequest(token), CancellationToken.None);

        Assert.Equal("CheckedIn", result.Action);
        Assert.Equal(BookingStatus.CheckedIn, result.Status);
        Assert.NotNull(result.CheckInTimestamp);
    }

    [Fact]
    public async Task ProcessQrAsync_requires_checkout_confirmation_for_checked_in_booking()
    {
        var fixture = await CreateBookingFixtureAsync();
        var booking = await AddBookingAsync(fixture, BookingStatus.CheckedIn);
        var staff = TestData.User(role: Role.Guard);
        await using (var seedContext = fixture.Database.CreateContext())
        {
            seedContext.Users.Add(staff);
            seedContext.ParkingLotStaffs.Add(new ParkingLotStaff { UserId = staff.Id, ParkingLotId = fixture.ParkingLot.Id });
            await seedContext.SaveChangesAsync();
        }
        string token;
        await using (var driverContext = fixture.Database.CreateContext())
        {
            token = (await new BookingService(driverContext, new TestCurrentUserService(fixture.Driver.Id))
                .GenerateQrTokenAsync(booking.Id, CancellationToken.None)).QrToken;
        }
        await using var staffContext = fixture.Database.CreateContext();
        var result = await new BookingService(staffContext, new TestCurrentUserService(staff.Id))
            .ProcessQrAsync(new VerifyQrRequest(token), CancellationToken.None);

        Assert.Equal("CheckoutConfirmationRequired", result.Action);
        Assert.True(result.EstimatedTotal > 0);
        Assert.Equal(BookingStatus.CheckedIn, (await staffContext.Bookings.FindAsync(booking.Id))!.Status);
    }

    [Fact]
    public async Task GetForOwnerAsync_returns_requested_page_and_total_count()
    {
        var fixture = await CreateBookingFixtureAsync();
        await AddBookingAsync(fixture, BookingStatus.Completed, "OWNER1");
        await AddBookingAsync(fixture, BookingStatus.Completed, "OWNER2");
        await AddBookingAsync(fixture, BookingStatus.Completed, "OWNER3");
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(fixture.Owner.Id));

        var result = await service.GetForOwnerAsync(2, 2, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.PageIndex);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetForCurrentStaffAsync_returns_requested_page_and_total_count()
    {
        var fixture = await CreateBookingFixtureAsync();
        var staff = TestData.User(role: Role.Guard);
        await AddBookingAsync(fixture, BookingStatus.Completed, "STAFF1");
        await AddBookingAsync(fixture, BookingStatus.Completed, "STAFF2");
        await AddBookingAsync(fixture, BookingStatus.Completed, "STAFF3");
        await using (var seedContext = fixture.Database.CreateContext())
        {
            seedContext.Users.Add(staff);
            seedContext.ParkingLotStaffs.Add(new ParkingLotStaff { UserId = staff.Id, ParkingLotId = fixture.ParkingLot.Id });
            await seedContext.SaveChangesAsync();
        }
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(staff.Id));

        var result = await service.GetForCurrentStaffAsync(2, 2, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.PageIndex);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetAllAdminAsync_returns_pagination_metadata_in_correct_fields()
    {
        var fixture = await CreateBookingFixtureAsync();
        await AddBookingAsync(fixture, BookingStatus.Completed, "ADMIN1");
        await AddBookingAsync(fixture, BookingStatus.Completed, "ADMIN2");
        await AddBookingAsync(fixture, BookingStatus.Completed, "ADMIN3");
        await using var context = fixture.Database.CreateContext();
        var service = new BookingService(context, new TestCurrentUserService(Guid.NewGuid()));

        var result = await service.GetAllAdminAsync(2, 2, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.PageIndex);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
    }

    private static async Task<BookingFixture> CreateBookingFixtureAsync(ParkingSlotStatus slotStatus = ParkingSlotStatus.Available)
    {
        var database = await TestDatabase.CreateInMemoryAsync();
        var owner = TestData.User(role: Role.ParkingOwner);
        var driver = TestData.User();
        var vehicle = TestData.Vehicle(driver.Id);
        var parkingLot = TestData.ParkingLot(owner.Id, firstBlockPrice: 50_000, availableSlots: slotStatus == ParkingSlotStatus.Available ? 1 : 0);
        var floor = TestData.Floor(parkingLot.Id);
        var slot = TestData.Slot(floor.Id, slotStatus);

        await using var context = database.CreateContext();
        context.Users.AddRange(owner, driver);
        context.Vehicles.Add(vehicle);
        context.ParkingLots.Add(parkingLot);
        context.ParkingFloors.Add(floor);
        context.ParkingSlots.Add(slot);
        await context.SaveChangesAsync();

        return new BookingFixture(database, owner, driver, vehicle, parkingLot, floor, slot);
    }

    private static async Task<Booking> AddBookingAsync(BookingFixture fixture, BookingStatus status, string? code = null)
    {
        await using var context = fixture.Database.CreateContext();
        var booking = TestData.Booking(fixture.Driver.Id, fixture.Vehicle.Id, fixture.ParkingLot.Id, fixture.Slot.Id, status);
        if (code is not null)
        {
            booking.BookingCode = code;
        }

        if (status is BookingStatus.Pending or BookingStatus.CheckedIn)
        {
            var slot = await context.ParkingSlots.SingleAsync(s => s.Id == fixture.Slot.Id);
            slot.Status = ParkingSlotStatus.Occupied;
        }

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        return booking;
    }

    private sealed record BookingFixture(
        TestDatabase Database,
        User Owner,
        User Driver,
        Vehicle Vehicle,
        ParkingLot ParkingLot,
        ParkingFloor Floor,
        ParkingSlot Slot);
}
