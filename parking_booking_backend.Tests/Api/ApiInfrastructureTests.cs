using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using parking_booking_backend.Tests.Infrastructure;
using Xunit;

namespace parking_booking_backend.Tests.Api;

public sealed class ApiInfrastructureTests : IClassFixture<ParkingBookingApiFactory>
{
    private readonly ParkingBookingApiFactory _factory;

    public ApiInfrastructureTests(ParkingBookingApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Application_exposes_all_documented_api_and_health_routes()
    {
        _ = _factory.CreateClient();
        var endpointSources = _factory.Services.GetServices<EndpointDataSource>();
        var routes = endpointSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => endpoint.RoutePattern.RawText)
            .Where(route => route is not null && (route.TrimStart('/').StartsWith("api/") || route.TrimStart('/').StartsWith("health/")))
            .ToList();

        Assert.Equal(59, routes.Count);
        Assert.Contains("api/bookings/recent-parking-lots", routes);
        Assert.Contains("api/bookings/admin/all", routes);
        Assert.Contains("api/bookings/{id:guid}/no-show", routes);
        Assert.Contains("api/parking-lots/{id:guid}/staff/by-phone", routes);
        Assert.Contains("api/users", routes);
        Assert.Contains(routes, route => route!.TrimStart('/') == "health/live");
        Assert.Contains(routes, route => route!.TrimStart('/') == "health/ready");
    }

    [Theory]
    [InlineData("/api/bookings/me")]
    [InlineData("/api/bookings/recent-parking-lots")]
    [InlineData("/api/bookings/staff")]
    [InlineData("/api/bookings/owner")]
    [InlineData("/api/bookings/admin/all")]
    [InlineData("/api/bookings/00000000-0000-0000-0000-000000000001/qr")]
    [InlineData("/api/parking-lots/nearby?lat=10&lng=106")]
    [InlineData("/api/parking-lots/bounds?minLat=10&maxLat=11&minLng=106&maxLng=107")]
    [InlineData("/api/parking-lots/search?keyword=test")]
    [InlineData("/api/parking-lots/staff/me")]
    [InlineData("/api/parking-lots/owner/me")]
    [InlineData("/api/parking-lots/owner/staff")]
    [InlineData("/api/parking-lots/admin/all")]
    [InlineData("/api/parking-lots/00000000-0000-0000-0000-000000000001")]
    [InlineData("/api/parking-lots/00000000-0000-0000-0000-000000000001/floors")]
    [InlineData("/api/parking-lots/00000000-0000-0000-0000-000000000001/floors/00000000-0000-0000-0000-000000000002/slots")]
    [InlineData("/api/users/me")]
    [InlineData("/api/users")]
    [InlineData("/api/vehicles")]
    [InlineData("/api/monthly-passes/me")]
    [InlineData("/api/wallets/me")]
    [InlineData("/api/layout-templates")]
    [InlineData("/api/vouchers/valid")]
    public async Task Protected_get_endpoints_reject_anonymous_requests(string path)
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public static IEnumerable<object[]> ProtectedWriteEndpoints()
    {
        const string id = "00000000-0000-0000-0000-000000000001";
        const string secondId = "00000000-0000-0000-0000-000000000002";
        yield return [HttpMethod.Post, "/api/bookings"];
        yield return [HttpMethod.Post, $"/api/bookings/{id}/check-in"];
        yield return [HttpMethod.Post, $"/api/bookings/{id}/apply-voucher"];
        yield return [HttpMethod.Post, $"/api/bookings/{id}/check-out"];
        yield return [HttpMethod.Post, $"/api/bookings/{id}/cancel"];
        yield return [HttpMethod.Post, $"/api/bookings/{id}/no-show"];
        yield return [HttpMethod.Post, "/api/bookings/verify-qr"];
        yield return [HttpMethod.Post, "/api/parking-lots"];
        yield return [HttpMethod.Put, $"/api/parking-lots/{id}"];
        yield return [HttpMethod.Put, $"/api/parking-lots/{id}/approve"];
        yield return [HttpMethod.Post, $"/api/parking-lots/{id}/report"];
        yield return [HttpMethod.Post, $"/api/parking-lots/{id}/staff"];
        yield return [HttpMethod.Post, $"/api/parking-lots/{id}/staff/by-phone"];
        yield return [HttpMethod.Post, $"/api/parking-lots/{id}/staff/create"];
        yield return [HttpMethod.Delete, $"/api/parking-lots/{id}/staff/{secondId}"];
        yield return [HttpMethod.Post, $"/api/parking-lots/{id}/floors"];
        yield return [HttpMethod.Put, $"/api/parking-lots/{id}/floors/{secondId}/slots"];
        yield return [HttpMethod.Put, "/api/users/me"];
        yield return [HttpMethod.Put, $"/api/users/{id}/lock"];
        yield return [HttpMethod.Post, "/api/users"];
        yield return [HttpMethod.Post, "/api/vehicles"];
        yield return [HttpMethod.Put, $"/api/vehicles/{id}"];
        yield return [HttpMethod.Delete, $"/api/vehicles/{id}"];
        yield return [HttpMethod.Post, "/api/monthly-passes"];
        yield return [HttpMethod.Post, "/api/wallets/deposit"];
        yield return [HttpMethod.Post, "/api/payments/webhook"];
        yield return [HttpMethod.Post, "/api/reviews"];
        yield return [HttpMethod.Post, "/api/vouchers"];
        yield return [HttpMethod.Put, $"/api/vouchers/{id}"];
        yield return [HttpMethod.Delete, $"/api/vouchers/{id}"];
    }

    [Theory]
    [MemberData(nameof(ProtectedWriteEndpoints))]
    public async Task Protected_write_endpoints_reject_anonymous_requests(HttpMethod method, string path)
    {
        using var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(new { })
        };

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_and_verify_are_anonymous_and_return_a_token()
    {
        using var isolatedFactory = new ParkingBookingApiFactory();
        using var client = isolatedFactory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new { phoneNumber = "0933999999" });
        var verify = await client.PostAsJsonAsync("/api/auth/verify", new
        {
            phoneNumber = "0933999999",
            otp = "123456",
            fullName = "Integration Driver"
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);
        Assert.Contains("token", await verify.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Health_live_and_ready_return_only_status()
    {
        using var client = _factory.CreateClient();

        var live = await client.GetAsync("/health/live");
        var ready = await client.GetAsync("/health/ready");
        var liveBody = await live.Content.ReadAsStringAsync();
        var readyBody = await ready.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, live.StatusCode);
        Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
        Assert.Contains("status", liveBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("status", readyBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("exception", readyBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection", readyBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_rate_limit_returns_429_and_retry_after()
    {
        using var isolatedFactory = new ParkingBookingApiFactory();
        using var client = isolatedFactory.CreateClient();
        HttpResponseMessage? rejected = null;

        for (var request = 0; request < 6; request++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new { phoneNumber = "0933888888" });
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                rejected = response;
                break;
            }
        }

        Assert.NotNull(rejected);
        Assert.True(rejected.Headers.Contains("Retry-After"));
    }
}
