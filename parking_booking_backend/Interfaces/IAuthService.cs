using parking_booking_backend.DTOs;

namespace parking_booking_backend.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> VerifyAsync(VerifyOtpRequest request, CancellationToken cancellationToken);
}
