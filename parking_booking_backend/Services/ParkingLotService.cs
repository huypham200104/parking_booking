using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class ParkingLotService : IParkingLotService
{
    private const int GeofenceMeters = 100;
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly GeometryFactory _geometryFactory = new(new PrecisionModel(), 4326);

    public ParkingLotService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetNearbyAsync(NearbyParkingLotsQuery query, CancellationToken cancellationToken)
    {
        var userLocation = _geometryFactory.CreatePoint(new Coordinate(query.Lng, query.Lat));
        var radiusMeters = query.RadiusKm * 1000;

        return await _dbContext.ParkingLots
            .AsNoTracking()
            .Where(p => p.Status == ParkingLotStatus.Active && p.Location.Distance(userLocation) <= radiusMeters)
            .OrderBy(p => p.Location.Distance(userLocation))
            .Select(p => new ParkingLotSummaryResponse(
                p.Id,
                p.Name,
                p.Address,
                p.Location.Y,
                p.Location.X,
                p.TotalSlots,
                p.AvailableSlots,
                p.FirstBlockPrice,
                p.FirstBlockHours,
                p.MaxHeight,
                p.HasRoof,
                p.Is24_7,
                p.AverageRating,
                p.Status,
                p.Location.Distance(userLocation) / 1000))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetInBoundsAsync(ParkingLotsInBoundsQuery query, CancellationToken cancellationToken)
    {
        var minLat = Math.Min(query.MinLat, query.MaxLat);
        var maxLat = Math.Max(query.MinLat, query.MaxLat);
        var minLng = Math.Min(query.MinLng, query.MaxLng);
        var maxLng = Math.Max(query.MinLng, query.MaxLng);
        var userLocation = query.UserLat.HasValue && query.UserLng.HasValue
            ? _geometryFactory.CreatePoint(new Coordinate(query.UserLng.Value, query.UserLat.Value))
            : null;

        var parkingLots = await _dbContext.ParkingLots
            .AsNoTracking()
            .Where(p =>
                p.Status == ParkingLotStatus.Active &&
                p.Location.Y >= minLat &&
                p.Location.Y <= maxLat &&
                p.Location.X >= minLng &&
                p.Location.X <= maxLng)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Address,
                Latitude = p.Location.Y,
                Longitude = p.Location.X,
                p.TotalSlots,
                p.AvailableSlots,
                p.FirstBlockPrice,
                p.FirstBlockHours,
                p.MaxHeight,
                p.HasRoof,
                p.Is24_7,
                p.AverageRating,
                p.Status,
                DistanceKm = userLocation == null ? (double?)null : p.Location.Distance(userLocation) / 1000
            })
            .OrderBy(p => p.DistanceKm ?? 0)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return parkingLots
            .Select(p => new ParkingLotSummaryResponse(
                p.Id,
                p.Name,
                p.Address,
                p.Latitude,
                p.Longitude,
                p.TotalSlots,
                p.AvailableSlots,
                p.FirstBlockPrice,
                p.FirstBlockHours,
                p.MaxHeight,
                p.HasRoof,
                p.Is24_7,
                p.AverageRating,
                p.Status,
                p.DistanceKm))
            .ToList();
    }

    public async Task<ParkingLotDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var parkingLot = await _dbContext.ParkingLots
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ParkingLotDetailResponse(
                p.Id,
                p.OwnerId,
                p.Name,
                p.Address,
                p.Description,
                p.CoverImageUrl,
                p.ContactPhone,
                p.Location.Y,
                p.Location.X,
                p.TotalSlots,
                p.AvailableSlots,
                p.FirstBlockPrice,
                p.FirstBlockHours,
                p.OvernightPrice,
                p.MaxHeight,
                p.HasRoof,
                p.Is24_7,
                p.OpenTime,
                p.CloseTime,
                p.AverageRating,
                p.Status))
            .FirstOrDefaultAsync(cancellationToken);

        return parkingLot ?? throw new ApiException("Parking lot was not found.", StatusCodes.Status404NotFound);
    }

    public async Task ReportAsync(Guid parkingLotId, CrowdsourceReportRequest request, CancellationToken cancellationToken)
    {
        var parkingLot = await _dbContext.ParkingLots.FirstOrDefaultAsync(p => p.Id == parkingLotId, cancellationToken)
            ?? throw new ApiException("Parking lot was not found.", StatusCodes.Status404NotFound);

        var userLocation = _geometryFactory.CreatePoint(new Coordinate(request.CurrentLng, request.CurrentLat));
        if (parkingLot.Location.Distance(userLocation) > GeofenceMeters)
        {
            throw new ApiException("You must be near this parking lot to report its status.", StatusCodes.Status403Forbidden);
        }

        _dbContext.CrowdsourceReports.Add(new CrowdsourceReport
        {
            UserId = _currentUser.UserId,
            ParkingLotId = parkingLotId,
            ReportedStatus = request.Status,
            ReportedAt = DateTime.UtcNow,
            IsProcessed = false
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStaffAsync(Guid parkingLotId, AddParkingLotStaffRequest request, CancellationToken cancellationToken)
    {
        var parkingLot = await _dbContext.ParkingLots.FirstOrDefaultAsync(p => p.Id == parkingLotId, cancellationToken)
            ?? throw new ApiException("Parking lot was not found.", StatusCodes.Status404NotFound);

        if (parkingLot.OwnerId != _currentUser.UserId)
        {
            throw new ApiException("Only the parking lot owner can add staff.", StatusCodes.Status403Forbidden);
        }

        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new ApiException("Staff user was not found.", StatusCodes.Status404NotFound);
        }

        var exists = await _dbContext.ParkingLotStaffs.AnyAsync(s => s.ParkingLotId == parkingLotId && s.UserId == request.UserId, cancellationToken);
        if (exists)
        {
            return;
        }

        _dbContext.ParkingLotStaffs.Add(new ParkingLotStaff { ParkingLotId = parkingLotId, UserId = request.UserId });
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
