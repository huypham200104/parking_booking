using System.ComponentModel.DataAnnotations;
using parking_booking_backend.Models;

namespace parking_booking_backend.DTOs;

public sealed record LoginRequest([Required, Phone] string PhoneNumber);

public sealed record VerifyOtpRequest(
    [Required, Phone] string PhoneNumber,
    [Required, StringLength(6, MinimumLength = 4)] string Otp,
    [StringLength(200, MinimumLength = 2)] string? FullName = null);

public sealed record AuthResponse(string Token, Role Role, Guid UserId);
