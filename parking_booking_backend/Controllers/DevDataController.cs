using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[ApiController]
[Route("api/dev/data")]
public sealed class DevDataController : ControllerBase
{
    private readonly IMockDataSeeder _mockDataSeeder;
    private readonly IWebHostEnvironment _environment;

    public DevDataController(IMockDataSeeder mockDataSeeder, IWebHostEnvironment environment)
    {
        _mockDataSeeder = mockDataSeeder;
        _environment = environment;
    }

    [HttpPost("seed")]
    public async Task<ActionResult<ApiResponse<SeedResult>>> Seed([FromQuery] bool recreate = false, CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await _mockDataSeeder.SeedAsync(recreate, cancellationToken);
        return Ok(ApiResponse<SeedResult>.Ok(result));
    }
}
