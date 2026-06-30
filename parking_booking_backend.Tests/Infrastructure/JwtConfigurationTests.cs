using Microsoft.Extensions.Configuration;
using parking_booking_backend.Extensions;
using Xunit;

namespace parking_booking_backend.Tests.Infrastructure;

public sealed class JwtConfigurationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("too-short")]
    public void RequireSecureKey_rejects_missing_or_short_values(string? key)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Key"] = key })
            .Build();

        Assert.Throws<InvalidOperationException>(() => JwtConfiguration.RequireSecureKey(configuration));
    }

    [Fact]
    public void RequireSecureKey_accepts_at_least_32_characters()
    {
        const string key = "12345678901234567890123456789012";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Jwt:Key"] = key })
            .Build();

        Assert.Equal(key, JwtConfiguration.RequireSecureKey(configuration));
    }
}
