using System;
using System.ComponentModel.DataAnnotations;

namespace parking_booking_backend.DTOs;

public record CreateReviewRequest(
    [Required] Guid BookingId,
    [Required][Range(1, 5)] int Rating,
    string? Comment
);

public record ReviewResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    int Rating,
    string? Comment,
    DateTime CreatedAt
);
