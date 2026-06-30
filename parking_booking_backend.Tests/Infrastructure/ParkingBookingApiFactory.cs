using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using parking_booking_backend.Data;
using parking_booking_backend.Services;

namespace parking_booking_backend.Tests.Infrastructure;

public sealed class ParkingBookingApiFactory : WebApplicationFactory<Program>
{
    public const string JwtKey = "integration-test-jwt-key-at-least-32-characters";

    public ParkingBookingApiFactory()
    {
        Environment.SetEnvironmentVariable("JWT__KEY", JwtKey);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<IDatabaseProvider>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase($"api-tests-{Guid.NewGuid()}"));
            services.RemoveAll<IDataProtectionProvider>();
            services.AddDataProtection().UseEphemeralDataProtectionProvider();

            var autoCancel = services.FirstOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType == typeof(BookingAutoCancelService));
            if (autoCancel is not null)
            {
                services.Remove(autoCancel);
            }

            services.PostConfigure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Clear();
                options.Registrations.Add(new HealthCheckRegistration(
                    "test-ready",
                    _ => new HealthyTestHealthCheck(),
                    HealthStatus.Unhealthy,
                    ["ready"]));
            });
        });
    }

    private sealed class HealthyTestHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
            => Task.FromResult(HealthCheckResult.Healthy());
    }
}
