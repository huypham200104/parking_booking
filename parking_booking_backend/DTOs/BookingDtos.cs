using System.ComponentModel.DataAnnotations;
using parking_booking_backend.Models;

namespace parking_booking_backend.DTOs;

public sealed record CreateBookingRequest(
    [Required] Guid ParkingSlotId,
    Guid? VehicleId,
    string? GuestLicensePlate);

public sealed record BookingResponse(
    Guid Id,
    Guid? UserId,
    Guid? VehicleId,
    string? GuestLicensePlate,
    Guid ParkingLotId,
    Guid ParkingSlotId,
    string BookingCode,
    DateTime BookingTimestamp,
    DateTime? CheckInTimestamp,
    DateTime? CheckOutTimestamp,
    BookingStatus Status,
    decimal? TotalPrice);

public sealed record BookingHistoryResponse(
    Guid Id,
    string BookingCode,
    string ParkingLotName,
    string ParkingLotAddress,
    string LicensePlate,
    DateTime BookingTimestamp,
    DateTime? CheckInTimestamp,
    DateTime? CheckOutTimestamp,
    BookingStatus Status,
    decimal? TotalPrice);

public sealed record StaffBookingResponse(
    Guid Id,
    string BookingCode,
    string ParkingLotName,
    string FloorName,
    string SlotName,
    string LicensePlate,
    DateTime BookingTimestamp,
    DateTime? CheckInTimestamp,
    DateTime? CheckOutTimestamp,
    BookingStatus Status,
    decimal? TotalPrice);

public sealed record ApplyVoucherRequest([Required] string VoucherCode);

public sealed record CheckOutRequest(bool UseWallet, bool CollectCash = false);

public sealed record CheckOutResponse(decimal TotalPrice, string? VietQrUrl, TransactionStatus Status, DateTime CheckOutTimestamp);
