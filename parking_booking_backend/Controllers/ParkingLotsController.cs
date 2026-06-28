using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/parking-lots")]
public sealed class ParkingLotsController : ControllerBase
{
    private readonly IParkingLotService _parkingLotService;
    private readonly ILayoutService _layoutService;

    public ParkingLotsController(IParkingLotService parkingLotService, ILayoutService layoutService)
    {
        _parkingLotService = parkingLotService;
        _layoutService = layoutService;
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>>> GetNearby([FromQuery] NearbyParkingLotsQuery query, CancellationToken cancellationToken)
    {
        var parkingLots = await _parkingLotService.GetNearbyAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>.Ok(parkingLots));
    }

    [HttpGet("bounds")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>>> GetInBounds([FromQuery] ParkingLotsInBoundsQuery query, CancellationToken cancellationToken)
    {
        var parkingLots = await _parkingLotService.GetInBoundsAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>.Ok(parkingLots));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ParkingLotDetailResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var parkingLot = await _parkingLotService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ParkingLotDetailResponse>.Ok(parkingLot));
    }

    [HttpPost("{id:guid}/report")]
    public async Task<ActionResult<ApiResponse<object>>> Report(Guid id, CrowdsourceReportRequest request, CancellationToken cancellationToken)
    {
        await _parkingLotService.ReportAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { reported = true }));
    }

    [HttpPost("{id:guid}/staff")]
    public async Task<ActionResult<ApiResponse<object>>> AddStaff(Guid id, AddParkingLotStaffRequest request, CancellationToken cancellationToken)
    {
        await _parkingLotService.AddStaffAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { added = true }));
    }

    [HttpGet("{id:guid}/floors")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingFloorResponse>>>> GetFloors(Guid id, CancellationToken cancellationToken)
    {
        var floors = await _layoutService.GetFloorsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParkingFloorResponse>>.Ok(floors));
    }

    [HttpPost("{id:guid}/floors")]
    public async Task<ActionResult<ApiResponse<ParkingFloorResponse>>> CreateFloor(Guid id, CreateParkingFloorRequest request, CancellationToken cancellationToken)
    {
        var floor = await _layoutService.CreateFloorAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetFloors), new { id }, ApiResponse<ParkingFloorResponse>.Ok(floor));
    }

    [HttpGet("{id:guid}/floors/{floorId:guid}/slots")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingSlotResponse>>>> GetSlots(Guid id, Guid floorId, CancellationToken cancellationToken)
    {
        var slots = await _layoutService.GetSlotsAsync(id, floorId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParkingSlotResponse>>.Ok(slots));
    }

    [HttpPut("{id:guid}/floors/{floorId:guid}/slots")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingSlotResponse>>>> SaveSlots(Guid id, Guid floorId, IReadOnlyCollection<UpsertParkingSlotRequest> request, CancellationToken cancellationToken)
    {
        var slots = await _layoutService.SaveSlotsAsync(id, floorId, request, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParkingSlotResponse>>.Ok(slots));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}/reviews")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ReviewResponse>>>> GetReviews(Guid id, [FromServices] IReviewService reviewService, CancellationToken cancellationToken)
    {
        var reviews = await reviewService.GetByParkingLotAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ReviewResponse>>.Ok(reviews));
    }
}
