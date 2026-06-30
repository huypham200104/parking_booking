using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public static class BookingPolicy
{
    public static readonly TimeSpan CheckInWindow = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan ViolationWindow = TimeSpan.FromDays(30);
    public const int AllowedViolations = 3;

    public static DateTime GetCheckInDeadline(DateTime bookingTimestamp)
        => bookingTimestamp.Add(CheckInWindow);

    public static bool IsViolation(BookingStatus status)
        => status is BookingStatus.Cancelled or BookingStatus.NoShow;

    public static bool ShouldLock(int violationCount)
        => violationCount > AllowedViolations;
}
