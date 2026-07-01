namespace parking_booking_backend.Exceptions;

public class ApiException : Exception
{
    public ApiException(string message, int statusCode = StatusCodes.Status400BadRequest, string? errorCode = null, Guid? activeBookingId = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        ActiveBookingId = activeBookingId;
    }

    public int StatusCode { get; }
    public string? ErrorCode { get; }
    public Guid? ActiveBookingId { get; }
}
