using parking_booking_backend.DTOs;

namespace parking_booking_backend.Interfaces;

public interface IParkingLotService
{
    Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetNearbyAsync(NearbyParkingLotsQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetInBoundsAsync(ParkingLotsInBoundsQuery query, CancellationToken cancellationToken);

    Task<ParkingLotDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task ReportAsync(Guid parkingLotId, CrowdsourceReportRequest request, CancellationToken cancellationToken);

    Task AddStaffAsync(Guid parkingLotId, AddParkingLotStaffRequest request, CancellationToken cancellationToken);
}

public interface ILayoutService
{
    Task<IReadOnlyCollection<LayoutTemplateResponse>> GetTemplatesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ParkingFloorResponse>> GetFloorsAsync(Guid parkingLotId, CancellationToken cancellationToken);

    Task<ParkingFloorResponse> CreateFloorAsync(Guid parkingLotId, CreateParkingFloorRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ParkingSlotResponse>> GetSlotsAsync(Guid parkingLotId, Guid floorId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ParkingSlotResponse>> SaveSlotsAsync(Guid parkingLotId, Guid floorId, IReadOnlyCollection<UpsertParkingSlotRequest> request, CancellationToken cancellationToken);
}

public interface IBookingService
{
    Task<IReadOnlyCollection<BookingHistoryResponse>> GetMineAsync(CancellationToken cancellationToken);

    Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken);

    Task<BookingResponse> CheckInAsync(Guid id, CancellationToken cancellationToken);

    Task<BookingResponse> ApplyVoucherAsync(Guid id, ApplyVoucherRequest request, CancellationToken cancellationToken);

    Task<CheckOutResponse> CheckOutAsync(Guid id, CheckOutRequest request, CancellationToken cancellationToken);

    Task CancelAsync(Guid id, CancellationToken cancellationToken);

    Task<BookingQrResponse> GenerateQrTokenAsync(Guid id, CancellationToken cancellationToken);

    Task<VerifyQrResponse> VerifyQrTokenAsync(VerifyQrRequest request, CancellationToken cancellationToken);
}

public interface IUserService
{
    Task<UserResponse> GetMeAsync(CancellationToken cancellationToken);
}

public interface IVehicleService
{
    Task<IReadOnlyCollection<VehicleResponse>> GetMineAsync(CancellationToken cancellationToken);

    Task<VehicleResponse> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IMonthlyPassService
{
    Task<IReadOnlyCollection<MonthlyPassResponse>> GetMineAsync(CancellationToken cancellationToken);

    Task<MonthlyPassResponse> CreateAsync(CreateMonthlyPassRequest request, CancellationToken cancellationToken);
}

public interface IWalletService
{
    Task<WalletResponse> GetMineAsync(CancellationToken cancellationToken);

    Task<DepositResponse> CreateDepositAsync(DepositRequest request, CancellationToken cancellationToken);
}
