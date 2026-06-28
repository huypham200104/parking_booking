using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Services;

public sealed class UserService : IUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public UserService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<UserResponse> GetMeAsync(CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == _currentUser.UserId)
            .Select(u => new UserResponse(u.Id, u.PhoneNumber, u.FullName, u.Role, u.TrustScore))
            .FirstOrDefaultAsync(cancellationToken);

        return user ?? throw new ApiException("User was not found.", StatusCodes.Status404NotFound);
    }
}

