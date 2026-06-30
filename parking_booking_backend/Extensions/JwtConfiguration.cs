namespace parking_booking_backend.Extensions;

public static class JwtConfiguration
{
    public static string RequireSecureKey(IConfiguration configuration)
    {
        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
        {
            throw new InvalidOperationException("JWT__KEY is required and must contain at least 32 characters.");
        }

        return key;
    }
}
