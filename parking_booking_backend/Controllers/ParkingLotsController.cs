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

    [Authorize(Roles = "ParkingOwner")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ParkingLotDetailResponse>>> Create(CreateParkingLotRequest request, CancellationToken cancellationToken)
    {
        var result = await _parkingLotService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ParkingLotDetailResponse>.Ok(result));
    }

    [Authorize(Roles = "ParkingOwner")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ParkingLotDetailResponse>>> Update(Guid id, UpdateParkingLotRequest request, CancellationToken cancellationToken)
    {
        var result = await _parkingLotService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ParkingLotDetailResponse>.Ok(result));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<object>>> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _parkingLotService.ApproveAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { approved = true }));
    }

    [HttpGet("bounds")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>>> GetInBounds([FromQuery] ParkingLotsInBoundsQuery query, CancellationToken cancellationToken)
    {
        var parkingLots = await _parkingLotService.GetInBoundsAsync(query, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>.Ok(parkingLots));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<ParkingLotSearchResponse>>> Search([FromQuery] string keyword, CancellationToken cancellationToken)
    {
        var result = await _parkingLotService.SearchAsync(keyword, cancellationToken);
        return Ok(ApiResponse<ParkingLotSearchResponse>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ParkingLotDetailResponse>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var parkingLot = await _parkingLotService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ParkingLotDetailResponse>.Ok(parkingLot));
    }

    [Authorize(Roles = "Guard")]
    [HttpGet("staff/me")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>>> GetAssignedToCurrentStaff(CancellationToken cancellationToken)
    {
        var parkingLots = await _parkingLotService.GetAssignedToCurrentStaffAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>.Ok(parkingLots));
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

    [Authorize(Roles = "ParkingOwner")]
    [HttpPost("{id:guid}/staff/by-phone")]
    public async Task<ActionResult<ApiResponse<object>>> AddStaffByPhone(Guid id, AddParkingLotStaffByPhoneRequest request, CancellationToken cancellationToken)
    {
        await _parkingLotService.AddStaffByPhoneAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { added = true }));
    }

    [Authorize(Roles = "ParkingOwner")]
    [HttpPost("{id:guid}/staff/create")]
    public async Task<ActionResult<ApiResponse<object>>> CreateStaff(Guid id, CreateParkingLotStaffRequest request, CancellationToken cancellationToken)
    {
        await _parkingLotService.CreateStaffAsync(id, request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { created = true }));
    }

    [Authorize(Roles = "ParkingOwner")]
    [HttpDelete("{id:guid}/staff/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveStaff(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _parkingLotService.RemoveStaffAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { removed = true }));
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

    [Authorize(Roles = "ParkingOwner")]
    [HttpGet("owner/me")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>>> GetOwnedByMe(CancellationToken cancellationToken)
    {
        var parkingLots = await _parkingLotService.GetOwnedByMeAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>.Ok(parkingLots));
    }

    [Authorize(Roles = "ParkingOwner")]
    [HttpGet("owner/staff")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<OwnerStaffAssignmentResponse>>>> GetMyStaff(CancellationToken cancellationToken)
    {
        var staff = await _parkingLotService.GetMyStaffAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<OwnerStaffAssignmentResponse>>.Ok(staff));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<ParkingLotSummaryResponse>>>> GetAllAdmin([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? keyword = null, CancellationToken cancellationToken = default)
    {
        var result = await _parkingLotService.GetAllAdminAsync(page, size, keyword, cancellationToken);
        return Ok(ApiResponse<PaginationResponse<ParkingLotSummaryResponse>>.Ok(result));
    }
}
