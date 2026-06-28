using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ActionResult<ApiResponse<object>> Login(LoginRequest request)
    {
        return Ok(ApiResponse<object>.Ok(new { otpSent = true }, "OTP request accepted."));
    }

    [HttpPost("verify")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Verify(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.VerifyAsync(request, cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(response));
    }
}

