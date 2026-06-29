using System;
using System.ComponentModel.DataAnnotations;

namespace parking_booking_backend.DTOs;

public sealed record VoucherResponse(
    Guid Id,
    string Code,
    decimal? DiscountAmount,
    float? DiscountPercentage,
    decimal? MaxDiscount,
    DateTime ExpiryDate,
    int UsageLimit);

public sealed record CreateVoucherRequest(
    [Required, StringLength(20, MinimumLength = 3)] string Code,
    [Range(0, 1000000)] decimal? DiscountAmount,
    [Range(0, 100)] float? DiscountPercentage,
    [Range(0, 1000000)] decimal? MaxDiscount,
    [Required] DateTime ExpiryDate,
    [Range(1, 10000)] int UsageLimit);

public sealed record UpdateVoucherRequest(
    [Range(0, 1000000)] decimal? DiscountAmount,
    [Range(0, 100)] float? DiscountPercentage,
    [Range(0, 1000000)] decimal? MaxDiscount,
    [Required] DateTime ExpiryDate,
    [Range(1, 10000)] int UsageLimit);
