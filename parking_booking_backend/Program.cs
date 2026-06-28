using parking_booking_backend.Extensions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Middlewares;

// Khởi tạo 1 object builder là 1 cái kho chứa khổng lồ
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
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
app.UseAuthorization();

// API 
app.MapControllers();

app.Run();
