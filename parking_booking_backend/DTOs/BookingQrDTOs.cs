using System;

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
