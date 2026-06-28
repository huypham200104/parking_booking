using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Services;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class CurrentUserServiceTests
{
    [Fact]
    public void UserId_reads_sub_claim_before_header()
    {
        var claimUserId = Guid.NewGuid();
        var headerUserId = Guid.NewGuid();
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", claimUserId.ToString())]))
        };
        context.Request.Headers["X-User-Id"] = headerUserId.ToString();

        var service = new CurrentUserService(new HttpContextAccessor { HttpContext = context });

        Assert.True(service.IsAuthenticated);
        Assert.Equal(claimUserId, service.UserId);
    }

    [Fact]
    public void UserId_reads_header_when_claim_is_missing()
    {
        var userId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers["X-User-Id"] = userId.ToString();

        var service = new CurrentUserService(new HttpContextAccessor { HttpContext = context });

        Assert.True(service.IsAuthenticated);
        Assert.Equal(userId, service.UserId);
    }

    [Fact]
    public void UserId_throws_when_user_is_missing()
    {
        var service = new CurrentUserService(new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

        var exception = Assert.Throws<ApiException>(() => service.UserId);
        Assert.Equal(StatusCodes.Status401Unauthorized, exception.StatusCode);
        Assert.False(service.IsAuthenticated);
    }
}

