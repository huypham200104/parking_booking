using parking_booking_backend.DTOs;

namespace parking_booking_backend.Interfaces;

public interface IParkingLotService
{
    Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetNearbyAsync(NearbyParkingLotsQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetInBoundsAsync(ParkingLotsInBoundsQuery query, CancellationToken cancellationToken);

    Task<ParkingLotSearchResponse> SearchAsync(string keyword, CancellationToken cancellationToken);

    Task<ParkingLotDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetAssignedToCurrentStaffAsync(CancellationToken cancellationToken);

    Task ReportAsync(Guid parkingLotId, CrowdsourceReportRequest request, CancellationToken cancellationToken);

    Task AddStaffAsync(Guid parkingLotId, AddParkingLotStaffRequest request, CancellationToken cancellationToken);

    Task AddStaffByPhoneAsync(Guid parkingLotId, AddParkingLotStaffByPhoneRequest request, CancellationToken cancellationToken);

    Task CreateStaffAsync(Guid parkingLotId, CreateParkingLotStaffRequest request, CancellationToken cancellationToken);

    Task RemoveStaffAsync(Guid parkingLotId, Guid userId, CancellationToken cancellationToken);

    Task<PaginationResponse<ParkingLotSummaryResponse>> GetAllAdminAsync(int pageIndex, int pageSize, string? keyword, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetOwnedByMeAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<OwnerStaffAssignmentResponse>> GetMyStaffAsync(CancellationToken cancellationToken);

    Task<ParkingLotDetailResponse> CreateAsync(CreateParkingLotRequest request, CancellationToken cancellationToken);

    Task<ParkingLotDetailResponse> UpdateAsync(Guid id, UpdateParkingLotRequest request, CancellationToken cancellationToken);

    Task ApproveAsync(Guid id, CancellationToken cancellationToken);
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

    Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetRecentCompletedParkingLotsAsync(CancellationToken cancellationToken);

    Task<PaginationResponse<StaffBookingResponse>> GetForCurrentStaffAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<PaginationResponse<StaffBookingResponse>> GetForOwnerAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<PaginationResponse<StaffBookingResponse>> GetAllAdminAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);

    Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken);

    Task<BookingResponse> CheckInAsync(Guid id, CancellationToken cancellationToken);

    Task<BookingResponse> ApplyVoucherAsync(Guid id, ApplyVoucherRequest request, CancellationToken cancellationToken);

    Task<CheckOutResponse> CheckOutAsync(Guid id, CheckOutRequest request, CancellationToken cancellationToken);

    Task CancelAsync(Guid id, CancellationToken cancellationToken);

    Task MarkNoShowAsync(Guid id, CancellationToken cancellationToken);

    Task<BookingQrResponse> GenerateQrTokenAsync(Guid id, CancellationToken cancellationToken);

    Task<VerifyQrResponse> VerifyQrTokenAsync(VerifyQrRequest request, CancellationToken cancellationToken);

    Task<ProcessQrResponse> ProcessQrAsync(VerifyQrRequest request, CancellationToken cancellationToken);
}

public interface IUserService
{
    Task<UserResponse> GetMeAsync(CancellationToken cancellationToken);

    Task<UserResponse> UpdateMeAsync(UpdateCurrentUserRequest request, CancellationToken cancellationToken);

    Task<PaginationResponse<AdminUserResponse>> GetAllAsync(int pageIndex, int pageSize, bool? hasPenalty, string? keyword, CancellationToken cancellationToken);

    Task ToggleLockAsync(Guid id, CancellationToken cancellationToken);

    Task<AdminUserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
}

public interface IVehicleService
{
    Task<IReadOnlyCollection<VehicleResponse>> GetMineAsync(CancellationToken cancellationToken);

    Task<VehicleResponse> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken);

    Task<VehicleResponse> UpdateAsync(Guid id, UpdateVehicleRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IVoucherService
{
    Task<IReadOnlyCollection<VoucherResponse>> GetValidVouchersAsync(CancellationToken cancellationToken);

    Task<VoucherResponse> CreateAsync(CreateVoucherRequest request, CancellationToken cancellationToken);

    Task<VoucherResponse> UpdateAsync(Guid id, UpdateVoucherRequest request, CancellationToken cancellationToken);

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

    Task<AdminWalletStatsResponse> GetAdminStatsAsync(CancellationToken cancellationToken);

    Task<PaginationResponse<AdminUserWalletResponse>> GetAdminUserWalletsAsync(int pageIndex, int pageSize, string? keyword, CancellationToken cancellationToken);
}
