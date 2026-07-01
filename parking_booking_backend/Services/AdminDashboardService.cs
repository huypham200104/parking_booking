using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class AdminDashboardService(ApplicationDbContext dbContext, TimeProvider timeProvider) : IAdminDashboardService
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    public async Task<AdminDashboardResponse> GetAsync(CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow();
        var vietnamToday = utcNow.ToOffset(VietnamOffset).Date;
        var startUtc = new DateTimeOffset(vietnamToday, VietnamOffset).UtcDateTime;
        var endUtc = startUtc.AddDays(1);

        var bookingsToday = await dbContext.Bookings.AsNoTracking()
            .CountAsync(booking => booking.BookingTimestamp >= startUtc && booking.BookingTimestamp < endUtc, cancellationToken);
        var totalParkingLots = await dbContext.ParkingLots.AsNoTracking().CountAsync(cancellationToken);
        var activeParkingLots = await dbContext.ParkingLots.AsNoTracking()
            .CountAsync(lot => lot.Status == ParkingLotStatus.Active, cancellationToken);
        var pendingParkingLots = await dbContext.ParkingLots.AsNoTracking()
            .CountAsync(lot => lot.Status == ParkingLotStatus.PendingApproval, cancellationToken);
        var newUsersToday = await dbContext.Users.AsNoTracking()
            .CountAsync(user => user.CreatedAt >= startUtc && user.CreatedAt < endUtc, cancellationToken);
        var recentBookings = await dbContext.Bookings.AsNoTracking()
            .OrderByDescending(booking => booking.BookingTimestamp)
            .Take(4)
            .Select(booking => new AdminRecentBookingResponse(
                booking.Id,
                booking.BookingCode,
                booking.User != null ? booking.User.FullName : "Khách vãng lai",
                booking.ParkingLot!.Name,
                booking.Vehicle != null ? booking.Vehicle.LicensePlate : booking.GuestLicensePlate ?? "N/A",
                booking.Status,
                booking.TotalPrice,
                booking.BookingTimestamp))
            .ToListAsync(cancellationToken);

        return new AdminDashboardResponse(
            bookingsToday,
            activeParkingLots,
            totalParkingLots,
            pendingParkingLots,
            newUsersToday,
            recentBookings);
    }
}
