namespace parking_booking_backend.DTOs;

public sealed record ApiResponse<T>(bool Success, T? Data, string? Message)
{
    public static ApiResponse<T> Ok(T data, string? message = null) => new(true, data, message);

    public static ApiResponse<T> Fail(string message) => new(false, default, message);
}

public sealed record PaginationResponse<T>(IReadOnlyCollection<T> Items, int TotalCount, int PageIndex, int PageSize, int TotalPages);

