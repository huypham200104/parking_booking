using NetTopologySuite.Geometries;
using parking_booking_backend.Models;

namespace parking_booking_backend.Tests.Infrastructure;

public static class TestData
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    public static User User(Guid? id = null, Role role = Role.Driver, string? phone = null)
    {
        var userId = id ?? Guid.NewGuid();
        var phoneNumber = phone ?? $"09{Math.Abs(userId.GetHashCode()) % 100000000:00000000}";

        return new User
        {
            Id = userId,
            PhoneNumber = phoneNumber,
            FullName = $"User {phoneNumber}",
            Role = role
        };
    }

    public static Wallet Wallet(Guid userId, decimal balance = 0)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = balance
        };

    public static Vehicle Vehicle(Guid userId, string plate = "51F-123.45", bool isDefault = true)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LicensePlate = plate,
            VehicleType = VehicleType.Sedan,
            IsDefault = isDefault
        };

    public static ParkingLot ParkingLot(Guid ownerId, decimal firstBlockPrice = 20_000, int availableSlots = 1)
        => new()
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = "Test Parking",
            Address = "District 1",
            Description = "Test parking lot",
            CoverImageUrl = "https://example.local/cover.jpg",
            ContactPhone = "02839990000",
            Location = GeometryFactory.CreatePoint(new Coordinate(106.7000, 10.7750)),
            TotalSlots = 1,
            AvailableSlots = availableSlots,
            FirstBlockPrice = firstBlockPrice,
            FirstBlockHours = 1,
            HasRoof = true,
            Is24_7 = true,
            AverageRating = 4.5f,
            Status = ParkingLotStatus.Active
        };

    public static LayoutTemplate LayoutTemplate()
        => new()
        {
            Id = Guid.NewGuid(),
            Name = "Template A",
            ImageUrl = "https://example.local/template.png",
            Description = "Template"
        };

    public static ParkingFloor Floor(Guid parkingLotId, Guid? templateId = null, string name = "B1")
        => new()
        {
            Id = Guid.NewGuid(),
            ParkingLotId = parkingLotId,
            TemplateId = templateId,
            FloorName = name
        };

    public static ParkingSlot Slot(Guid floorId, ParkingSlotStatus status = ParkingSlotStatus.Available)
        => new()
        {
            Id = Guid.NewGuid(),
            ParkingFloorId = floorId,
            SlotName = "A01",
            Status = status,
            VehicleType = SlotVehicleType.Car,
            PositionX = 10,
            PositionY = 20,
            Width = 60,
            Height = 100,
            Rotation = 0
        };

    public static Booking Booking(
        Guid userId,
        Guid vehicleId,
        Guid parkingLotId,
        Guid parkingSlotId,
        BookingStatus status = BookingStatus.Pending)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            VehicleId = vehicleId,
            ParkingLotId = parkingLotId,
            ParkingSlotId = parkingSlotId,
            BookingCode = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
            BookingTimestamp = DateTime.UtcNow.AddMinutes(-10),
            CheckInTimestamp = status == BookingStatus.CheckedIn ? DateTime.UtcNow.AddHours(-2) : null,
            Status = status
        };

    public static Voucher Voucher(string code = "FREE10K", decimal? amount = 10_000, float? percentage = null, decimal? maxDiscount = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            DiscountAmount = amount,
            DiscountPercentage = percentage,
            MaxDiscount = maxDiscount,
            ExpiryDate = DateTime.UtcNow.AddDays(30),
            UsageLimit = 10
        };
}
