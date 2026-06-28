using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/monthly-passes")]
public sealed class MonthlyPassesController : ControllerBase
{
    private readonly IMonthlyPassService _monthlyPassService;

    public MonthlyPassesController(IMonthlyPassService monthlyPassService)
    {
        _monthlyPassService = monthlyPassService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<MonthlyPassResponse>>>> GetMine(CancellationToken cancellationToken)
    {
        var passes = await _monthlyPassService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<MonthlyPassResponse>>.Ok(passes));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MonthlyPassResponse>>> Create(CreateMonthlyPassRequest request, CancellationToken cancellationToken)
    {
        var pass = await _monthlyPassService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMine), ApiResponse<MonthlyPassResponse>.Ok(pass));
    }
}

