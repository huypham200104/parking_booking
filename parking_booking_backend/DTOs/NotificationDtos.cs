namespace parking_booking_backend.DTOs;

public sealed record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt);
