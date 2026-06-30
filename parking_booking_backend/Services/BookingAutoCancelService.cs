using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using parking_booking_backend.Data;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public class BookingAutoCancelService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingAutoCancelService> _logger;

    public BookingAutoCancelService(IServiceProvider serviceProvider, ILogger<BookingAutoCancelService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BookingAutoCancelService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAutoCancelAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Ignore task cancellation on app shutdown
                break;
            }
            catch (ObjectDisposedException)
            {
                // Ignore if DI container is disposed during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing BookingAutoCancelService.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Ignore task cancellation on app shutdown
                break;
            }
        }
    }

    private async Task ProcessAutoCancelAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;
        var thresholdTime = now.Subtract(BookingPolicy.CheckInWindow);

        var expiredBookings = await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Pending && b.BookingTimestamp <= thresholdTime)
            .Include(b => b.User)
            .Include(b => b.ParkingSlot)
            .Include(b => b.ParkingLot)
            .ToListAsync(cancellationToken);

        if (!expiredBookings.Any())
        {
            return;
        }

        var expiredBookingIds = expiredBookings.Select(booking => booking.Id).ToList();
        var affectedUserIds = expiredBookings
            .Where(booking => booking.UserId.HasValue)
            .Select(booking => booking.UserId!.Value)
            .Distinct()
            .ToList();

        var windowStart = now.Subtract(BookingPolicy.ViolationWindow);
        var previousViolationCounts = affectedUserIds.Count == 0
            ? new Dictionary<Guid, int>()
            : await dbContext.Bookings
                .Where(booking => booking.UserId.HasValue
                    && affectedUserIds.Contains(booking.UserId.Value)
                    && !expiredBookingIds.Contains(booking.Id)
                    && booking.BookingTimestamp >= windowStart
                    && (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.NoShow))
                .GroupBy(booking => booking.UserId!.Value)
                .Select(group => new { UserId = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.UserId, item => item.Count, cancellationToken);

        foreach (var booking in expiredBookings)
        {
            booking.Status = BookingStatus.NoShow;
            if (booking.ParkingSlot != null)
            {
                booking.ParkingSlot.Status = ParkingSlotStatus.Available;
            }

            if (booking.ParkingLot != null)
            {
                booking.ParkingLot.AvailableSlots = Math.Min(
                    booking.ParkingLot.TotalSlots,
                    booking.ParkingLot.AvailableSlots + 1);
            }
        }

        foreach (var userBookings in expiredBookings
                     .Where(booking => booking.UserId.HasValue && booking.User != null)
                     .GroupBy(booking => booking.UserId!.Value))
        {
            var previousCount = previousViolationCounts.GetValueOrDefault(userBookings.Key);
            if (BookingPolicy.ShouldLock(previousCount + userBookings.Count()))
            {
                var user = userBookings.First().User!;
                user.IsLocked = true;
                _logger.LogWarning(
                    "User {UserId} has been locked after more than {AllowedViolations} booking violations in 30 days.",
                    userBookings.Key,
                    BookingPolicy.AllowedViolations);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"Auto-cancelled {expiredBookings.Count} bookings.");
    }
}
