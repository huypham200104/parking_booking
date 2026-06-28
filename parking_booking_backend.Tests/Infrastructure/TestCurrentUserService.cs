using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Tests.Infrastructure;

public sealed class TestCurrentUserService : ICurrentUserService
{
    public TestCurrentUserService(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; }

    public bool IsAuthenticated => true;
}

