using parking_booking_backend.DTOs;

namespace parking_booking_backend.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyCollection<NotificationResponse>> GetMineAsync(bool unreadOnly, CancellationToken cancellationToken);
    Task MarkReadAsync(Guid id, CancellationToken cancellationToken);
    Task MarkAllReadAsync(CancellationToken cancellationToken);
    Task CreateForUserAsync(string phoneNumber, string title, string message, CancellationToken cancellationToken);
}

public interface IFavouriteParkingLotService
{
    Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetMineAsync(CancellationToken cancellationToken);
    Task AddAsync(Guid parkingLotId, CancellationToken cancellationToken);
    Task RemoveAsync(Guid parkingLotId, CancellationToken cancellationToken);
}
