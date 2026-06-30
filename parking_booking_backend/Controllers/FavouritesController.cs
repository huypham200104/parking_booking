using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize(Roles = "Driver")]
[ApiController]
[Route("api/favourites")]
public sealed class FavouritesController(IFavouriteParkingLotService favouriteService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>>> GetMine(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>.Ok(await favouriteService.GetMineAsync(cancellationToken)));

    [HttpPost("{parkingLotId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Add(Guid parkingLotId, CancellationToken cancellationToken)
    {
        await favouriteService.AddAsync(parkingLotId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { favourite = true }));
    }

    [HttpDelete("{parkingLotId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Remove(Guid parkingLotId, CancellationToken cancellationToken)
    {
        await favouriteService.RemoveAsync(parkingLotId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { favourite = false }));
    }
}
