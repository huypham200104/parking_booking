using System.ComponentModel.DataAnnotations;
using parking_booking_backend.Models;

namespace parking_booking_backend.DTOs;

public sealed record UserResponse(Guid Id, string PhoneNumber, string FullName, Role Role, int TrustScore);

public sealed record AdminUserResponse(Guid Id, string PhoneNumber, string FullName, Role Role, int TrustScore, bool IsLocked, DateTime CreatedAt, int PenaltyCount);

public sealed record UpdateCurrentUserRequest(
    [Required, StringLength(200, MinimumLength = 2)] string FullName);

public sealed record VehicleResponse(Guid Id, Guid UserId, string LicensePlate, VehicleType VehicleType, bool IsDefault);

public sealed record CreateVehicleRequest(
    [Required, StringLength(20, MinimumLength = 5)] string LicensePlate,
    [Required] VehicleType VehicleType,
    bool IsDefault);

public sealed record UpdateVehicleRequest(
    [Required] VehicleType VehicleType,
    bool IsDefault);

public sealed record MonthlyPassResponse(
    Guid Id,
    Guid UserId,
    Guid VehicleId,
    Guid ParkingLotId,
    DateTime StartDate,
    DateTime EndDate,
    decimal Price,
    MonthlyPassStatus Status,
    string? PaymentUrl);

public sealed record CreateMonthlyPassRequest(
    [Required] Guid VehicleId,
    [Required] Guid ParkingLotId,
    [Range(1, 365)] int DurationDays,
    bool UseWallet);

public sealed record WalletResponse(Guid Id, Guid UserId, decimal Balance, IReadOnlyCollection<WalletTransactionResponse> Transactions);

public sealed record WalletTransactionResponse(Guid Id, decimal Amount, WalletTransactionType Type, string ReferenceId);

public sealed record DepositRequest([Range(10000, 50000000)] decimal Amount);

public sealed record DepositResponse(decimal Amount, string VietQrUrl);

public sealed record CreateUserRequest(
    [Required, StringLength(15, MinimumLength = 10)] string PhoneNumber,
    [Required, StringLength(200, MinimumLength = 2)] string FullName,
    [Required] Role Role,
    [Required, StringLength(100, MinimumLength = 6)] string Password);
