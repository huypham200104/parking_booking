using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public VehicleService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<VehicleResponse>> GetMineAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.UserId == _currentUser.UserId)
            .OrderByDescending(v => v.IsDefault)
            .ThenBy(v => v.LicensePlate)
            .Select(v => ToResponse(v))
            .ToListAsync(cancellationToken);
    }

    public async Task<VehicleResponse> CreateAsync(CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        var licensePlate = request.LicensePlate.Trim().ToUpperInvariant();
        var exists = await _dbContext.Vehicles.AnyAsync(v => v.UserId == _currentUser.UserId && v.LicensePlate == licensePlate, cancellationToken);
        if (exists)
        {
            throw new ApiException("License plate already exists.", StatusCodes.Status409Conflict);
        }

        if (request.IsDefault)
        {
            await _dbContext.Vehicles
                .Where(v => v.UserId == _currentUser.UserId && v.IsDefault)
                .ExecuteUpdateAsync(updates => updates.SetProperty(v => v.IsDefault, false), cancellationToken);
        }

        var vehicle = new Vehicle
        {
            UserId = _currentUser.UserId,
            LicensePlate = licensePlate,
            VehicleType = request.VehicleType,
            IsDefault = request.IsDefault
        };

        _dbContext.Vehicles.Add(vehicle);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(vehicle);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await _dbContext.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == _currentUser.UserId, cancellationToken);
        if (vehicle == null)
        {
            throw new ApiException("Vehicle was not found.", StatusCodes.Status404NotFound);
        }

        // Soft delete (BaseEntity pattern handled by DbContext or manually set here)
        vehicle.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static VehicleResponse ToResponse(Vehicle vehicle)
        => new(vehicle.Id, vehicle.UserId, vehicle.LicensePlate, vehicle.VehicleType, vehicle.IsDefault);
}

