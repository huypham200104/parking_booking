using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;


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

    public async Task<UserResponse> UpdateMeAsync(UpdateCurrentUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken)
            ?? throw new ApiException("User was not found.", StatusCodes.Status404NotFound);

        if (user.Role == Role.Guard)
        {
            throw new ApiException("Nhân viên không được phép chỉnh sửa thông tin cá nhân.", StatusCodes.Status403Forbidden);
        }

        var fullName = request.FullName.Trim();

        user.FullName = fullName;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UserResponse(user.Id, user.PhoneNumber, user.FullName, user.Role, user.TrustScore);
    }

    public async Task<PaginationResponse<AdminUserResponse>> GetAllAsync(int pageIndex, int pageSize, bool? hasPenalty, CancellationToken cancellationToken)
    {
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _dbContext.Users.AsNoTracking();

        if (hasPenalty.HasValue && hasPenalty.Value)
        {
            query = query.Where(u => u.Bookings.Count(b => b.Status == BookingStatus.NoShow) > 0);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserResponse(u.Id, u.PhoneNumber, u.FullName, u.Role, u.TrustScore, u.IsLocked, u.CreatedAt, u.Bookings.Count(b => b.Status == BookingStatus.NoShow)))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginationResponse<AdminUserResponse>(users, totalCount, pageIndex, pageSize, totalPages);
    }

    public async Task ToggleLockAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken)
            ?? throw new ApiException("User was not found.", StatusCodes.Status404NotFound);

        // Cannot lock/unlock themselves
        if (user.Id == _currentUser.UserId)
        {
            throw new ApiException("Cannot modify own lock status.", StatusCodes.Status403Forbidden);
        }

        user.IsLocked = !user.IsLocked;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
