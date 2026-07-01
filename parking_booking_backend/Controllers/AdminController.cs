using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public sealed class AdminController(IAdminDashboardService dashboardService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<AdminDashboardResponse>>> GetDashboard(CancellationToken cancellationToken)
        => Ok(ApiResponse<AdminDashboardResponse>.Ok(await dashboardService.GetAsync(cancellationToken)));
}
