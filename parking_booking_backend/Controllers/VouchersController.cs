using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/vouchers")]
public sealed class VouchersController : ControllerBase
{
    private readonly IVoucherService _voucherService;

    public VouchersController(IVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [HttpGet("valid")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<VoucherResponse>>>> GetValidVouchers(CancellationToken cancellationToken)
    {
        var vouchers = await _voucherService.GetValidVouchersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<VoucherResponse>>.Ok(vouchers));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<VoucherResponse>>> Create(CreateVoucherRequest request, CancellationToken cancellationToken)
    {
        var voucher = await _voucherService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetValidVouchers), ApiResponse<VoucherResponse>.Ok(voucher));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<VoucherResponse>>> Update(Guid id, UpdateVoucherRequest request, CancellationToken cancellationToken)
    {
        var voucher = await _voucherService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<VoucherResponse>.Ok(voucher));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _voucherService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { deleted = true }));
    }
}
