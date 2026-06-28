using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/vehicles")]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<VehicleResponse>>>> GetMine(CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<VehicleResponse>>.Ok(vehicles));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<VehicleResponse>>> Create(CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetMine), ApiResponse<VehicleResponse>.Ok(vehicle));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _vehicleService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}

