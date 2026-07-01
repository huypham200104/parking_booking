using parking_booking_backend.Models;

namespace parking_booking_backend.DTOs;

public sealed record AdminRecentBookingResponse(
    Guid Id,
    string BookingCode,
    string UserName,
    string ParkingLotName,
    string LicensePlate,
    BookingStatus Status,
    decimal? TotalPrice,
    DateTime BookingTimestamp);

public sealed record AdminDashboardResponse(
    int BookingsToday,
    int ActiveParkingLots,
    int TotalParkingLots,
    int PendingParkingLots,
    int NewUsersToday,
    IReadOnlyCollection<AdminRecentBookingResponse> RecentBookings);
