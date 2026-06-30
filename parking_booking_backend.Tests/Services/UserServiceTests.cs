using Microsoft.AspNetCore.Http;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Models;
using parking_booking_backend.Services;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class UserServiceTests
{
    [Fact]
    public async Task GetMeAsync_returns_current_user()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var user = TestData.User(role: Role.Admin);
        await using (var context = database.CreateContext())
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        await using var queryContext = database.CreateContext();
        var service = new UserService(queryContext, new TestCurrentUserService(user.Id));

        var result = await service.GetMeAsync(CancellationToken.None);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(Role.Admin, result.Role);
    }

    [Fact]
    public async Task GetMeAsync_throws_when_user_does_not_exist()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        await using var context = database.CreateContext();
        var service = new UserService(context, new TestCurrentUserService(Guid.NewGuid()));

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.GetMeAsync(CancellationToken.None));
        Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateMeAsync_updates_only_current_user_profile()
    {
        await using var database = await TestDatabase.CreateInMemoryAsync();
        var user = TestData.User(role: Role.Driver);
        await using (var context = database.CreateContext())
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        await using var updateContext = database.CreateContext();
        var service = new UserService(updateContext, new TestCurrentUserService(user.Id));
        var result = await service.UpdateMeAsync(
            new(" Nguyen Van Driver "),
            CancellationToken.None);

        Assert.Equal(user.PhoneNumber, result.PhoneNumber);
        Assert.Equal("Nguyen Van Driver", result.FullName);
        Assert.Equal(Role.Driver, result.Role);
        Assert.Equal(user.TrustScore, result.TrustScore);
    }

}
