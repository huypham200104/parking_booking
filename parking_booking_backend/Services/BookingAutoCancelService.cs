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

        var thresholdTime = DateTime.UtcNow.AddMinutes(-10);

        var expiredBookings = await dbContext.Bookings
            .Where(b => b.Status == BookingStatus.Pending && b.BookingTimestamp <= thresholdTime)
            .Include(b => b.User)
            .ToListAsync(cancellationToken);

        if (!expiredBookings.Any())
        {
            return;
        }

        var today = DateTime.UtcNow.Date;

        foreach (var booking in expiredBookings)
        {
            booking.Status = BookingStatus.Cancelled;

            if (booking.User != null && !booking.User.IsLocked)
            {
                // Count cancelled bookings today for this user (not including this one yet)
                var cancelledCountToday = await dbContext.Bookings
                    .Where(b => b.UserId == booking.UserId 
                                && b.Status == BookingStatus.Cancelled
                                && b.BookingTimestamp.Date == today)
                    .CountAsync(cancellationToken);

                if (cancelledCountToday + 1 >= 3)
                {
                    booking.User.IsLocked = true;
                    _logger.LogWarning($"User {booking.UserId} has been locked due to 3 or more cancellations today.");
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"Auto-cancelled {expiredBookings.Count} bookings.");
    }
}
