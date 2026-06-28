namespace parking_booking_backend.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }
}

