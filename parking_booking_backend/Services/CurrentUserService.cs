using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using System.Security.Claims;

namespace parking_booking_backend.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private const string UserIdHeaderName = "X-User-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            if (!TryGetUserId(out var userId))
            {
                throw new ApiException($"Unauthorized access.", StatusCodes.Status401Unauthorized);
            }

            return userId;
        }
    }

    public bool IsAuthenticated => TryGetUserId(out _);

    private bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return false;
        }

        var claimValue = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? httpContext.User.FindFirst("sub")?.Value;

        if (Guid.TryParse(claimValue, out userId))
        {
            return true;
        }

        return httpContext.Request.Headers.TryGetValue(UserIdHeaderName, out var headerValue)
            && Guid.TryParse(headerValue.FirstOrDefault(), out userId);
    }
}
