using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public sealed class NotificationsController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<NotificationResponse>>>> GetMine(
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
        => Ok(ApiResponse<IReadOnlyCollection<NotificationResponse>>.Ok(
            await notificationService.GetMineAsync(unreadOnly, cancellationToken)));

    [HttpPut("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await notificationService.MarkReadAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { read = true }));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAllRead(CancellationToken cancellationToken)
    {
        await notificationService.MarkAllReadAsync(cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { read = true }));
    }

    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> SendNotificationToUser(
        [FromBody] CreateUserNotificationRequest request,
        CancellationToken cancellationToken)
    {
        await notificationService.CreateForUserAsync(request.PhoneNumber, request.Title, request.Message, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { sent = true }));
    }
}
