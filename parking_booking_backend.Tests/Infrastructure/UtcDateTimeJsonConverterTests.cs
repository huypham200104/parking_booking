using System.Text.Json;
using parking_booking_backend.Infrastructure;
using parking_booking_backend.Services;
using Xunit;

namespace parking_booking_backend.Tests.Infrastructure;

public sealed class UtcDateTimeJsonConverterTests
{
    [Fact]
    public void Converter_treats_unspecified_database_value_as_utc_and_writes_z_suffix()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UtcDateTimeJsonConverter());
        var value = new DateTime(2026, 7, 1, 4, 42, 0, DateTimeKind.Unspecified);

        var json = JsonSerializer.Serialize(value, options);

        Assert.Contains("2026-07-01T04:42:00.0000000Z", json, StringComparison.Ordinal);
        Assert.Equal("11:42", BookingPolicy.FormatVietnamTime(value));
    }
}
