using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class VoucherService : IVoucherService
{
    private readonly ApplicationDbContext _dbContext;

    public VoucherService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<VoucherResponse>> GetValidVouchersAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Vouchers
            .AsNoTracking()
            .Where(v => v.ExpiryDate > DateTime.UtcNow && v.UsageLimit > 0)
            .OrderBy(v => v.ExpiryDate)
            .Select(v => new VoucherResponse(
                v.Id,
                v.Code,
                v.DiscountAmount,
                v.DiscountPercentage,
                v.MaxDiscount,
                v.ExpiryDate,
                v.UsageLimit))
            .ToListAsync(cancellationToken);
    }

    public async Task<VoucherResponse> CreateAsync(CreateVoucherRequest request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var exists = await _dbContext.Vouchers.AnyAsync(v => v.Code == code, cancellationToken);
        if (exists)
        {
            throw new ApiException("Voucher code already exists.", StatusCodes.Status409Conflict);
        }

        if (request.DiscountAmount == null && request.DiscountPercentage == null)
        {
            throw new ApiException("Must provide either DiscountAmount or DiscountPercentage.", StatusCodes.Status400BadRequest);
        }

        var voucher = new Voucher
        {
            Code = code,
            DiscountAmount = request.DiscountAmount,
            DiscountPercentage = request.DiscountPercentage,
            MaxDiscount = request.MaxDiscount,
            ExpiryDate = request.ExpiryDate.ToUniversalTime(),
            UsageLimit = request.UsageLimit
        };

        _dbContext.Vouchers.Add(voucher);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(voucher);
    }

    public async Task<VoucherResponse> UpdateAsync(Guid id, UpdateVoucherRequest request, CancellationToken cancellationToken)
    {
        var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new ApiException("Voucher was not found.", StatusCodes.Status404NotFound);

        if (request.DiscountAmount == null && request.DiscountPercentage == null)
        {
            throw new ApiException("Must provide either DiscountAmount or DiscountPercentage.", StatusCodes.Status400BadRequest);
        }

        voucher.DiscountAmount = request.DiscountAmount;
        voucher.DiscountPercentage = request.DiscountPercentage;
        voucher.MaxDiscount = request.MaxDiscount;
        voucher.ExpiryDate = request.ExpiryDate.ToUniversalTime();
        voucher.UsageLimit = request.UsageLimit;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(voucher);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(v => v.Id == id, cancellationToken)
            ?? throw new ApiException("Voucher was not found.", StatusCodes.Status404NotFound);

        var isUsed = await _dbContext.Bookings.AnyAsync(b => b.VoucherId == id, cancellationToken);
        if (isUsed)
        {
            throw new ApiException("Cannot delete a voucher that has already been used.", StatusCodes.Status409Conflict);
        }

        _dbContext.Vouchers.Remove(voucher);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static VoucherResponse ToResponse(Voucher voucher)
    {
        return new VoucherResponse(
            voucher.Id,
            voucher.Code,
            voucher.DiscountAmount,
            voucher.DiscountPercentage,
            voucher.MaxDiscount,
            voucher.ExpiryDate,
            voucher.UsageLimit);
    }
}
