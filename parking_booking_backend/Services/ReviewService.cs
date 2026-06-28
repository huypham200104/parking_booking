using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public ReviewService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<ReviewResponse> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var booking = await _dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken)
            ?? throw new ApiException("Booking was not found.", StatusCodes.Status404NotFound);

        if (booking.UserId != _currentUser.UserId)
        {
            throw new ApiException("You can only review your own bookings.", StatusCodes.Status403Forbidden);
        }

        if (booking.Status != BookingStatus.Completed)
        {
            throw new ApiException("Only completed bookings can be reviewed.", StatusCodes.Status409Conflict);
        }

        var existingReview = await _dbContext.Reviews
            .AnyAsync(r => r.BookingId == request.BookingId, cancellationToken);
            
        if (existingReview)
        {
            throw new ApiException("You have already reviewed this booking.", StatusCodes.Status409Conflict);
        }

        var user = await _dbContext.Users.FirstAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        var review = new Review
        {
            UserId = _currentUser.UserId,
            ParkingLotId = booking.ParkingLotId,
            BookingId = booking.Id,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ReviewResponse(
            review.Id,
            user.Id,
            user.FullName,
            review.Rating,
            review.Comment,
            review.CreatedAt
        );
    }

    public async Task<IReadOnlyCollection<ReviewResponse>> GetByParkingLotAsync(Guid parkingLotId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ParkingLots.AnyAsync(p => p.Id == parkingLotId, cancellationToken);
        if (!exists)
        {
            throw new ApiException("Parking lot was not found.", StatusCodes.Status404NotFound);
        }

        return await _dbContext.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.ParkingLotId == parkingLotId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewResponse(
                r.Id,
                r.UserId,
                r.User!.FullName,
                r.Rating,
                r.Comment,
                r.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
