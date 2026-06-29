using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetMe(CancellationToken cancellationToken)
    {
        var user = await _userService.GetMeAsync(cancellationToken);
        return Ok(ApiResponse<UserResponse>.Ok(user));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateMe(UpdateCurrentUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateMeAsync(request, cancellationToken);
        return Ok(ApiResponse<UserResponse>.Ok(user, "Personal information updated successfully."));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<AdminUserResponse>>>> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] bool? hasPenalty = null, CancellationToken cancellationToken = default)
    {
        var users = await _userService.GetAllAsync(page, size, hasPenalty, cancellationToken);
        return Ok(ApiResponse<PaginationResponse<AdminUserResponse>>.Ok(users));
    }

    [HttpPut("{id:guid}/lock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> ToggleLock(Guid id, CancellationToken cancellationToken)
    {
        await _userService.ToggleLockAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!, "User lock status toggled successfully."));
    }
}
