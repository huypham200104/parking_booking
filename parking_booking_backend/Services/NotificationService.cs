using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Services;

public sealed class NotificationService(ApplicationDbContext dbContext, ICurrentUserService currentUser) : INotificationService
{
    public async Task<IReadOnlyCollection<NotificationResponse>> GetMineAsync(bool unreadOnly, CancellationToken cancellationToken)
    {
        var query = dbContext.Notifications.AsNoTracking().Where(item => item.UserId == currentUser.UserId);
        if (unreadOnly)
        {
            query = query.Where(item => !item.IsRead);
        }

        return await query
            .OrderByDescending(item => item.CreatedAt)
            .Take(50)
            .Select(item => new NotificationResponse(item.Id, item.Title, item.Message, item.IsRead, item.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkReadAsync(Guid id, CancellationToken cancellationToken)
    {
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == currentUser.UserId, cancellationToken)
            ?? throw new ApiException("Không tìm thấy thông báo.", StatusCodes.Status404NotFound);
        notification.IsRead = true;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(CancellationToken cancellationToken)
    {
        var notifications = await dbContext.Notifications
            .Where(item => item.UserId == currentUser.UserId && !item.IsRead)
            .ToListAsync(cancellationToken);
        notifications.ForEach(item => item.IsRead = true);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateForUserAsync(string phoneNumber, string title, string message, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken)
            ?? throw new ApiException("Không tìm thấy người dùng với số điện thoại này.", StatusCodes.Status404NotFound);

        var notification = new Models.Notification
        {
            UserId = user.Id,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
