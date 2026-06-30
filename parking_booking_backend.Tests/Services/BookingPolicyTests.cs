using parking_booking_backend.Models;
using parking_booking_backend.Services;
using Xunit;

namespace parking_booking_backend.Tests.Services;

public sealed class BookingPolicyTests
{
    [Fact]
    public void CheckInDeadline_is_ten_minutes_after_booking_time()
    {
        var bookingTime = new DateTime(2026, 6, 30, 16, 55, 0, DateTimeKind.Utc);

        var deadline = BookingPolicy.GetCheckInDeadline(bookingTime);

        Assert.Equal(new DateTime(2026, 6, 30, 17, 5, 0, DateTimeKind.Utc), deadline);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(3, false)]
    [InlineData(4, true)]
    public void Account_is_locked_only_after_more_than_three_violations(int count, bool expected)
        => Assert.Equal(expected, BookingPolicy.ShouldLock(count));

    [Theory]
    [InlineData(BookingStatus.Cancelled, true)]
    [InlineData(BookingStatus.NoShow, true)]
    [InlineData(BookingStatus.Pending, false)]
    [InlineData(BookingStatus.CheckedIn, false)]
    [InlineData(BookingStatus.Completed, false)]
    public void Only_cancelled_and_no_show_are_violations(BookingStatus status, bool expected)
        => Assert.Equal(expected, BookingPolicy.IsViolation(status));
}
