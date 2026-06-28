namespace parking_booking_backend.Interfaces;

public interface IMockDataSeeder
{
    Task<SeedResult> SeedAsync(bool recreateDatabase, CancellationToken cancellationToken);
}

public sealed record SeedResult(
    bool Created,
    int Users,
    int ParkingLots,
    int ParkingFloors,
    int ParkingSlots,
    int Vehicles,
    int Vouchers,
    string Message);
