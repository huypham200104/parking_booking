using System.ComponentModel.DataAnnotations;
using parking_booking_backend.Models;

namespace parking_booking_backend.DTOs;

public sealed record ParkingLotSummaryResponse(
    Guid Id,
    string Name,
    string Address,
    double Latitude,
    double Longitude,
    int TotalSlots,
    int AvailableSlots,
    decimal FirstBlockPrice,
    int FirstBlockHours,
    decimal? MaxHeight,
    bool HasRoof,
    bool Is24_7,
    float AverageRating,
    ParkingLotStatus Status,
    double? DistanceKm);

public sealed record MapBoundsResponse(
    double MinLat,
    double MinLng,
    double MaxLat,
    double MaxLng);

public sealed record ParkingLotSearchResponse(
    IReadOnlyCollection<ParkingLotSummaryResponse> Results,
    MapBoundsResponse? MapBounds);

public sealed record ParkingLotDetailResponse(
    Guid Id,
    Guid OwnerId,
    string Name,
    string Address,
    string Description,
    string CoverImageUrl,
    string ContactPhone,
    double Latitude,
    double Longitude,
    int TotalSlots,
    int AvailableSlots,
    decimal FirstBlockPrice,
    int FirstBlockHours,
    decimal? OvernightPrice,
    decimal? MaxHeight,
    bool HasRoof,
    bool Is24_7,
    TimeSpan? OpenTime,
    TimeSpan? CloseTime,
    float AverageRating,
    ParkingLotStatus Status);

public sealed record NearbyParkingLotsQuery(
    [Range(-90, 90)] double Lat,
    [Range(-180, 180)] double Lng,
    [Range(0.1, 100)] double RadiusKm = 3);

public sealed record ParkingLotsInBoundsQuery(
    [Range(-90, 90)] double MinLat,
    [Range(-90, 90)] double MaxLat,
    [Range(-180, 180)] double MinLng,
    [Range(-180, 180)] double MaxLng,
    [Range(-90, 90)] double? UserLat = null,
    [Range(-180, 180)] double? UserLng = null);

public sealed record CrowdsourceReportRequest(
    [Required] ReportStatus Status,
    [Range(-90, 90)] double CurrentLat,
    [Range(-180, 180)] double CurrentLng);

public sealed record AddParkingLotStaffRequest([Required] Guid UserId);

public sealed record AddParkingLotStaffByPhoneRequest([Required, StringLength(20)] string PhoneNumber);

public sealed record CreateParkingLotStaffRequest(
    [Required, StringLength(200, MinimumLength = 2)] string FullName,
    [Required, StringLength(20)] string PhoneNumber);

public sealed record OwnerStaffAssignmentResponse(
    Guid UserId,
    string FullName,
    string PhoneNumber,
    bool IsLocked,
    DateTime CreatedAt,
    Guid ParkingLotId,
    string ParkingLotName);

public sealed record CreateParkingLotRequest(
    [Required, StringLength(100, MinimumLength = 3)] string Name,
    [Required, StringLength(200, MinimumLength = 5)] string Address,
    Guid? OwnerId,
    [Range(-90, 90)] double Latitude,
    [Range(-180, 180)] double Longitude,
    [Range(0, 1000000)] decimal FirstBlockPrice,
    [Range(1, 24)] int FirstBlockHours,
    bool Is24_7,
    [StringLength(20)] string ContactPhone);

public sealed record UpdateParkingLotRequest(
    [Required, StringLength(100, MinimumLength = 3)] string Name,
    [Required, StringLength(200, MinimumLength = 5)] string Address,
    [Range(-90, 90)] double Latitude,
    [Range(-180, 180)] double Longitude,
    [Range(0, 1000000)] decimal FirstBlockPrice,
    [Range(1, 24)] int FirstBlockHours,
    bool Is24_7,
    [StringLength(20)] string ContactPhone);
