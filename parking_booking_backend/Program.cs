using parking_booking_backend.Extensions;
using parking_booking_backend.Health;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Middlewares;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Security.Claims;
using System.Threading.RateLimiting;
using parking_booking_backend.Infrastructure;

DotEnvLoader.LoadForDevelopment();

// Khởi tạo 1 object builder là 1 cái kho chứa khổng lồ
var builder = WebApplication.CreateBuilder(args);

_ = JwtConfiguration.RequireSecureKey(builder.Configuration);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new UtcDateTimeJsonConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var value)
            ? Math.Max(1, (int)Math.Ceiling(value.TotalSeconds))
            : 60;
        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            status = StatusCodes.Status429TooManyRequests,
            title = "Too Many Requests",
            detail = "Bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau.",
            retryAfterSeconds = retryAfter
        }, cancellationToken);
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var method = httpContext.Request.Method;
        var identity = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        var (policy, permitLimit, window) = path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase)
            ? ("auth-login", 5, TimeSpan.FromMinutes(5))
            : path.Equals("/api/auth/verify", StringComparison.OrdinalIgnoreCase)
                ? ("auth-verify", 10, TimeSpan.FromMinutes(15))
                : HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method) || HttpMethods.IsPatch(method)
                    ? ("write", 30, TimeSpan.FromMinutes(1))
                    : ("default", 120, TimeSpan.FromMinutes(1));

        return RateLimitPartition.GetFixedWindowLimiter($"{policy}:{identity}", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddRedisServices(builder.Configuration);
builder.Services.AddParkingBookingServices(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: ["ready"])
    .AddCheck<RedisHealthCheck>("redis", tags: ["ready"]);

// object build đã được đóng gói xong và bắt đầu 
var app = builder.Build();

// Seed dữ liệu giả lập 
if (args.Contains("--seed", StringComparer.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<IMockDataSeeder>();
    var recreateDatabase = args.Contains("--recreate", StringComparer.OrdinalIgnoreCase);
    var result = await seeder.SeedAsync(recreateDatabase, CancellationToken.None);
    Console.WriteLine(result.Message);
    Console.WriteLine($"Users: {result.Users}, ParkingLots: {result.ParkingLots}, Floors: {result.ParkingFloors}, Slots: {result.ParkingSlots}, Vehicles: {result.Vehicles}, Vouchers: {result.Vouchers}");
    return;
}

// Các quy định được đặt ra khi app được chạy 
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("FrontendDev");

// Chỉ chạy openApi khi ở môi trường development 
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Cấu hình các quy định khi app được chạy
app.UseHttpsRedirection();

app.UseAuthentication();
// app.UseRateLimiter();
app.UseAuthorization();

// API 
app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = WriteHealthResponseAsync
}).AllowAnonymous().DisableRateLimiting();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponseAsync
}).AllowAnonymous().DisableRateLimiting();

app.Run();

static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";
    return context.Response.WriteAsJsonAsync(new { status = report.Status.ToString() });
}

public partial class Program;
