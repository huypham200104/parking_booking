using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BookingHistoryResponse>>>> GetMine(CancellationToken cancellationToken)
    {
        var bookings = await _bookingService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<BookingHistoryResponse>>.Ok(bookings));
    }

    [Authorize(Roles = "Driver")]
    [HttpGet("recent-parking-lots")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>>> GetRecentParkingLots(CancellationToken cancellationToken)
    {
        var parkingLots = await _bookingService.GetRecentCompletedParkingLotsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ParkingLotSummaryResponse>>.Ok(parkingLots));
    }

    [Authorize(Roles = "Guard")]
    [HttpGet("staff")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<StaffBookingResponse>>>> GetForStaff(CancellationToken cancellationToken)
    {
        var bookings = await _bookingService.GetForCurrentStaffAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<StaffBookingResponse>>.Ok(bookings));
    }

    [Authorize(Roles = "ParkingOwner")]
    [HttpGet("owner")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<StaffBookingResponse>>>> GetForOwner(CancellationToken cancellationToken)
    {
        var bookings = await _bookingService.GetForOwnerAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<StaffBookingResponse>>.Ok(bookings));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<StaffBookingResponse>>>> GetAllAdmin([FromQuery] int page = 1, [FromQuery] int size = 10, CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingService.GetAllAdminAsync(page, size, cancellationToken);
        return Ok(ApiResponse<PaginationResponse<StaffBookingResponse>>.Ok(bookings));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = booking.Id }, ApiResponse<BookingResponse>.Ok(booking));
    }

    [HttpPost("{id:guid}/check-in")]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> CheckIn(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CheckInAsync(id, cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(booking));
    }

    [HttpPost("{id:guid}/apply-voucher")]
    public async Task<ActionResult<ApiResponse<BookingResponse>>> ApplyVoucher(Guid id, ApplyVoucherRequest request, CancellationToken cancellationToken)
    {
        var booking = await _bookingService.ApplyVoucherAsync(id, request, cancellationToken);
        return Ok(ApiResponse<BookingResponse>.Ok(booking));
    }

    [HttpPost("{id:guid}/check-out")]
    public async Task<ActionResult<ApiResponse<CheckOutResponse>>> CheckOut(Guid id, CheckOutRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookingService.CheckOutAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CheckOutResponse>.Ok(result));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _bookingService.CancelAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { cancelled = true }));
    }

    [HttpGet("{id:guid}/qr")]
    public async Task<ActionResult<ApiResponse<BookingQrResponse>>> GetQrCode(Guid id, CancellationToken cancellationToken)
    {
        var response = await _bookingService.GenerateQrTokenAsync(id, cancellationToken);
        return Ok(ApiResponse<BookingQrResponse>.Ok(response));
    }

    [Authorize(Roles = "Guard")]
    [HttpPost("verify-qr")]
    public async Task<ActionResult<ApiResponse<VerifyQrResponse>>> VerifyQr(VerifyQrRequest request, CancellationToken cancellationToken)
    {
        var response = await _bookingService.VerifyQrTokenAsync(request, cancellationToken);
        if (!response.IsValid)
        {
            return BadRequest(ApiResponse<VerifyQrResponse>.Fail(response.Message ?? "Invalid QR code."));
        }
        return Ok(ApiResponse<VerifyQrResponse>.Ok(response));
    }
}
