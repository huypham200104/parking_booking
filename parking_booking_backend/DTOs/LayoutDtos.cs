using System.ComponentModel.DataAnnotations;
using parking_booking_backend.Models;

namespace parking_booking_backend.DTOs;

public sealed record LayoutTemplateResponse(Guid Id, string Name, string ImageUrl, string Description);

public sealed record ParkingFloorResponse(
    Guid Id,
    Guid ParkingLotId,
    string FloorName,
    Guid? TemplateId,
    string? CustomBackgroundImageUrl);

public sealed record CreateParkingFloorRequest(
    [Required, StringLength(100)] string FloorName,
    Guid? TemplateId,
    string? CustomBackgroundImageUrl);

public sealed record ParkingSlotResponse(
    Guid Id,
    Guid ParkingFloorId,
    string SlotName,
    ParkingSlotStatus Status,
    SlotVehicleType VehicleType,
    float PositionX,
    float PositionY,
    float Width,
    float Height,
    float Rotation);

public sealed record UpsertParkingSlotRequest(
    Guid? Id,
    [Required, StringLength(50)] string SlotName,
    ParkingSlotStatus Status,
    SlotVehicleType VehicleType,
    [Range(0, float.MaxValue)] float PositionX,
    [Range(0, float.MaxValue)] float PositionY,
    [Range(1, float.MaxValue)] float Width,
    [Range(1, float.MaxValue)] float Height,
    float Rotation);

