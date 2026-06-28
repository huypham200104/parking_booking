using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class LayoutService : ILayoutService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public LayoutService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<LayoutTemplateResponse>> GetTemplatesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.LayoutTemplates
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new LayoutTemplateResponse(t.Id, t.Name, t.ImageUrl, t.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ParkingFloorResponse>> GetFloorsAsync(Guid parkingLotId, CancellationToken cancellationToken)
    {
        await EnsureParkingLotExistsAsync(parkingLotId, cancellationToken);

        return await _dbContext.ParkingFloors
            .AsNoTracking()
            .Where(f => f.ParkingLotId == parkingLotId)
            .OrderBy(f => f.FloorName)
            .Select(f => ToFloorResponse(f))
            .ToListAsync(cancellationToken);
    }

    public async Task<ParkingFloorResponse> CreateFloorAsync(Guid parkingLotId, CreateParkingFloorRequest request, CancellationToken cancellationToken)
    {
        await EnsureCanManageParkingLotAsync(parkingLotId, cancellationToken);

        if (request.TemplateId.HasValue)
        {
            var templateExists = await _dbContext.LayoutTemplates.AnyAsync(t => t.Id == request.TemplateId.Value, cancellationToken);
            if (!templateExists)
            {
                throw new ApiException("Layout template was not found.", StatusCodes.Status404NotFound);
            }
        }

        var floor = new ParkingFloor
        {
            ParkingLotId = parkingLotId,
            FloorName = request.FloorName.Trim(),
            TemplateId = request.TemplateId,
            CustomBackgroundImageUrl = request.CustomBackgroundImageUrl
        };

        _dbContext.ParkingFloors.Add(floor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToFloorResponse(floor);
    }

    public async Task<IReadOnlyCollection<ParkingSlotResponse>> GetSlotsAsync(Guid parkingLotId, Guid floorId, CancellationToken cancellationToken)
    {
        await EnsureFloorBelongsToParkingLotAsync(parkingLotId, floorId, cancellationToken);

        return await _dbContext.ParkingSlots
            .AsNoTracking()
            .Where(s => s.ParkingFloorId == floorId)
            .OrderBy(s => s.SlotName)
            .Select(s => ToSlotResponse(s))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ParkingSlotResponse>> SaveSlotsAsync(Guid parkingLotId, Guid floorId, IReadOnlyCollection<UpsertParkingSlotRequest> request, CancellationToken cancellationToken)
    {
        await EnsureCanManageParkingLotAsync(parkingLotId, cancellationToken);
        await EnsureFloorBelongsToParkingLotAsync(parkingLotId, floorId, cancellationToken);

        var existingSlots = await _dbContext.ParkingSlots
            .Where(s => s.ParkingFloorId == floorId)
            .ToListAsync(cancellationToken);

        foreach (var item in request)
        {
            ParkingSlot? slot = null;
            if (item.Id.HasValue)
            {
                slot = existingSlots.FirstOrDefault(s => s.Id == item.Id.Value);
                if (slot is null)
                {
                    throw new ApiException($"Parking slot '{item.Id.Value}' was not found on this floor.", StatusCodes.Status404NotFound);
                }
            }

            if (slot is null)
            {
                slot = new ParkingSlot { ParkingFloorId = floorId };
                _dbContext.ParkingSlots.Add(slot);
            }

            slot.SlotName = item.SlotName.Trim();
            slot.Status = item.Status;
            slot.VehicleType = item.VehicleType;
            slot.PositionX = item.PositionX;
            slot.PositionY = item.PositionY;
            slot.Width = item.Width;
            slot.Height = item.Height;
            slot.Rotation = item.Rotation;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await SyncParkingLotCapacityAsync(parkingLotId, cancellationToken);

        return await GetSlotsAsync(parkingLotId, floorId, cancellationToken);
    }

    private async Task SyncParkingLotCapacityAsync(Guid parkingLotId, CancellationToken cancellationToken)
    {
        var parkingLot = await _dbContext.ParkingLots.FirstAsync(p => p.Id == parkingLotId, cancellationToken);
        var slots = await _dbContext.ParkingSlots
            .Where(s => s.ParkingFloor != null && s.ParkingFloor.ParkingLotId == parkingLotId)
            .ToListAsync(cancellationToken);

        parkingLot.TotalSlots = slots.Count;
        parkingLot.AvailableSlots = slots.Count(s => s.Status == ParkingSlotStatus.Available);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureParkingLotExistsAsync(Guid parkingLotId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ParkingLots.AnyAsync(p => p.Id == parkingLotId, cancellationToken);
        if (!exists)
        {
            throw new ApiException("Parking lot was not found.", StatusCodes.Status404NotFound);
        }
    }

    private async Task EnsureCanManageParkingLotAsync(Guid parkingLotId, CancellationToken cancellationToken)
    {
        var canManage = await _dbContext.ParkingLots.AnyAsync(p => p.Id == parkingLotId && p.OwnerId == _currentUser.UserId, cancellationToken)
            || await _dbContext.ParkingLotStaffs.AnyAsync(s => s.ParkingLotId == parkingLotId && s.UserId == _currentUser.UserId, cancellationToken);

        if (!canManage)
        {
            throw new ApiException("You cannot manage this parking lot.", StatusCodes.Status403Forbidden);
        }
    }

    private async Task EnsureFloorBelongsToParkingLotAsync(Guid parkingLotId, Guid floorId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ParkingFloors.AnyAsync(f => f.Id == floorId && f.ParkingLotId == parkingLotId, cancellationToken);
        if (!exists)
        {
            throw new ApiException("Parking floor was not found.", StatusCodes.Status404NotFound);
        }
    }

    private static ParkingFloorResponse ToFloorResponse(ParkingFloor floor)
        => new(floor.Id, floor.ParkingLotId, floor.FloorName, floor.TemplateId, floor.CustomBackgroundImageUrl);

    private static ParkingSlotResponse ToSlotResponse(ParkingSlot slot)
        => new(slot.Id, slot.ParkingFloorId, slot.SlotName, slot.Status, slot.VehicleType, slot.PositionX, slot.PositionY, slot.Width, slot.Height, slot.Rotation);
}
