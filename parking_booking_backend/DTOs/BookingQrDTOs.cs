using System;
using parking_booking_backend.Models;

namespace parking_booking_backend.DTOs;

public record BookingQrResponse(string QrToken);

public record VerifyQrRequest(string QrToken);

public record VerifyQrResponse(
    bool IsValid, 
    Guid? BookingId, 
    string? BookingCode,
    string? LicensePlate,
    string? Message
);

public sealed record ProcessQrResponse(
    Guid BookingId,
    string BookingCode,
    string LicensePlate,
    Guid ParkingLotId,
    string ParkingLotName,
    BookingStatus Status,
    string Action,
    decimal? EstimatedTotal,
    DateTime? CheckInTimestamp);
