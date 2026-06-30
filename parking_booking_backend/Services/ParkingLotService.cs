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

    public async Task<ParkingLotSearchResponse> SearchAsync(string keyword, CancellationToken cancellationToken)
    {
        var query = _dbContext.ParkingLots.AsNoTracking().Where(p => p.Status == ParkingLotStatus.Active);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.Trim().ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(lowerKeyword) || 
                p.Address.ToLower().Contains(lowerKeyword));
        }

        var results = await query
            .OrderBy(p => p.Name)
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
                null))
            .ToListAsync(cancellationToken);

        MapBoundsResponse? mapBounds = null;
        if (results.Any())
        {
            mapBounds = new MapBoundsResponse(
                MinLat: results.Min(r => r.Latitude),
                MinLng: results.Min(r => r.Longitude),
                MaxLat: results.Max(r => r.Latitude),
                MaxLng: results.Max(r => r.Longitude)
            );
        }

        return new ParkingLotSearchResponse(results, mapBounds);
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

    public async Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetAssignedToCurrentStaffAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ParkingLotStaffs
            .AsNoTracking()
            .Where(staff => staff.UserId == _currentUser.UserId)
            .Select(staff => staff.ParkingLot!)
            .OrderBy(parkingLot => parkingLot.Name)
            .Select(parkingLot => new ParkingLotSummaryResponse(
                parkingLot.Id,
                parkingLot.Name,
                parkingLot.Address,
                parkingLot.Location.Y,
                parkingLot.Location.X,
                parkingLot.TotalSlots,
                parkingLot.AvailableSlots,
                parkingLot.FirstBlockPrice,
                parkingLot.FirstBlockHours,
                parkingLot.MaxHeight,
                parkingLot.HasRoof,
                parkingLot.Is24_7,
                parkingLot.AverageRating,
                parkingLot.Status,
                null))
            .ToListAsync(cancellationToken);
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
        var parkingLot = await _dbContext.ParkingLots
            .FirstOrDefaultAsync(p => p.Id == parkingLotId, cancellationToken)
            ?? throw new ApiException("Parking lot was not found.", StatusCodes.Status404NotFound);

        if (parkingLot.OwnerId != _currentUser.UserId)
        {
            throw new ApiException("Only owner can add staff.", StatusCodes.Status403Forbidden);
        }

        var staff = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new ApiException("Staff user was not found.", StatusCodes.Status404NotFound);

        if (staff.Role != Role.Guard)
        {
            throw new ApiException("User is not a guard.", StatusCodes.Status400BadRequest);
        }

        var existingAssignment = await _dbContext.ParkingLotStaffs
            .AnyAsync(s => s.UserId == request.UserId, cancellationToken);

        if (existingAssignment)
        {
            throw new ApiException("Nhân viên này đã thuộc một bãi xe khác.", StatusCodes.Status409Conflict);
        }

        var assignment = new ParkingLotStaff { ParkingLotId = parkingLotId, UserId = request.UserId };
        _dbContext.ParkingLotStaffs.Add(assignment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddStaffByPhoneAsync(Guid parkingLotId, AddParkingLotStaffByPhoneRequest request, CancellationToken cancellationToken)
    {
        var staff = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(
            user => user.PhoneNumber == request.PhoneNumber.Trim() && user.Role == Role.Guard,
            cancellationToken) ?? throw new ApiException("Không tìm thấy tài khoản nhân viên với số điện thoại này.", StatusCodes.Status404NotFound);

        await AddStaffAsync(parkingLotId, new AddParkingLotStaffRequest(staff.Id), cancellationToken);
    }

    public async Task CreateStaffAsync(Guid parkingLotId, CreateParkingLotStaffRequest request, CancellationToken cancellationToken)
    {
        var ownedLot = await _dbContext.ParkingLots.AnyAsync(
            lot => lot.Id == parkingLotId && lot.OwnerId == _currentUser.UserId,
            cancellationToken);
        if (!ownedLot) throw new ApiException("Bạn không có quyền quản lý bãi xe này.", StatusCodes.Status403Forbidden);

        var phoneNumber = request.PhoneNumber.Trim();
        if (await _dbContext.Users.AnyAsync(user => user.PhoneNumber == phoneNumber, cancellationToken))
        {
            throw new ApiException("Số điện thoại này đã có tài khoản trong hệ thống.", StatusCodes.Status409Conflict);
        }

        var staff = new User { FullName = request.FullName.Trim(), PhoneNumber = phoneNumber, Role = Role.Guard };
        _dbContext.Users.Add(staff);
        _dbContext.ParkingLotStaffs.Add(new ParkingLotStaff { UserId = staff.Id, ParkingLotId = parkingLotId });
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveStaffAsync(Guid parkingLotId, Guid userId, CancellationToken cancellationToken)
    {
        var ownedLot = await _dbContext.ParkingLots.AnyAsync(
            lot => lot.Id == parkingLotId && lot.OwnerId == _currentUser.UserId,
            cancellationToken);
        if (!ownedLot) throw new ApiException("Bạn không có quyền quản lý bãi xe này.", StatusCodes.Status403Forbidden);

        var assignment = await _dbContext.ParkingLotStaffs.FirstOrDefaultAsync(
            item => item.ParkingLotId == parkingLotId && item.UserId == userId,
            cancellationToken) ?? throw new ApiException("Không tìm thấy phân công nhân viên.", StatusCodes.Status404NotFound);

        _dbContext.ParkingLotStaffs.Remove(assignment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginationResponse<ParkingLotSummaryResponse>> GetAllAdminAsync(int pageIndex, int pageSize, string? keyword, CancellationToken cancellationToken)
    {
        var query = _dbContext.ParkingLots.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.Trim().ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(lowerKeyword) || 
                p.Address.ToLower().Contains(lowerKeyword));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var results = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
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
                null))
            .ToListAsync(cancellationToken);

        return new PaginationResponse<ParkingLotSummaryResponse>(results, totalCount, pageIndex, pageSize, totalPages);
    }

    public async Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetOwnedByMeAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ParkingLots
            .AsNoTracking()
            .Where(p => p.OwnerId == _currentUser.UserId)
            .OrderBy(p => p.Name)
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
                null))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OwnerStaffAssignmentResponse>> GetMyStaffAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ParkingLotStaffs
            .AsNoTracking()
            .Where(s => s.ParkingLot != null && s.ParkingLot.OwnerId == _currentUser.UserId)
            .OrderBy(s => s.User!.FullName)
            .ThenBy(s => s.ParkingLot!.Name)
            .Select(s => new OwnerStaffAssignmentResponse(
                s.UserId,
                s.User!.FullName,
                s.User.PhoneNumber,
                s.User.IsLocked,
                s.User.CreatedAt,
                s.ParkingLotId,
                s.ParkingLot!.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<ParkingLotDetailResponse> CreateAsync(CreateParkingLotRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await _dbContext.Users.FirstAsync(u => u.Id == _currentUser.UserId, cancellationToken);
        var ownerId = currentUser.Role == Role.ParkingOwner
            ? currentUser.Id
            : request.OwnerId ?? throw new ApiException("Owner is required.", StatusCodes.Status400BadRequest);
        var owner = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == ownerId, cancellationToken)
            ?? throw new ApiException("Owner was not found.", StatusCodes.Status404NotFound);

        if (owner.Role != Role.ParkingOwner)
        {
            throw new ApiException("User is not a Parking Owner.", StatusCodes.Status400BadRequest);
        }

        var parkingLot = new ParkingLot
        {
            Name = request.Name,
            Address = request.Address,
            OwnerId = ownerId,
            Location = _geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude)),
            FirstBlockPrice = request.FirstBlockPrice,
            FirstBlockHours = request.FirstBlockHours,
            Is24_7 = request.Is24_7,
            ContactPhone = request.ContactPhone,
            Status = ParkingLotStatus.PendingApproval,
            TotalSlots = 0,
            AvailableSlots = 0,
            Description = string.Empty,
            CoverImageUrl = string.Empty,
            AverageRating = 0
        };

        _dbContext.ParkingLots.Add(parkingLot);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(parkingLot.Id, cancellationToken);
    }

    public async Task<ParkingLotDetailResponse> UpdateAsync(Guid id, UpdateParkingLotRequest request, CancellationToken cancellationToken)
    {
        var parkingLot = await _dbContext.ParkingLots.FirstOrDefaultAsync(
            lot => lot.Id == id && lot.OwnerId == _currentUser.UserId,
            cancellationToken) ?? throw new ApiException("Không tìm thấy bãi xe thuộc tài khoản này.", StatusCodes.Status404NotFound);

        parkingLot.Name = request.Name.Trim();
        parkingLot.Address = request.Address.Trim();
        parkingLot.Location = _geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));
        parkingLot.FirstBlockPrice = request.FirstBlockPrice;
        parkingLot.FirstBlockHours = request.FirstBlockHours;
        parkingLot.Is24_7 = request.Is24_7;
        parkingLot.ContactPhone = request.ContactPhone.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(parkingLot.Id, cancellationToken);
    }

    public async Task ApproveAsync(Guid id, CancellationToken cancellationToken)
    {
        var parkingLot = await _dbContext.ParkingLots.FirstOrDefaultAsync(
            lot => lot.Id == id,
            cancellationToken) ?? throw new ApiException("Bãi đỗ xe không tồn tại.", StatusCodes.Status404NotFound);

        parkingLot.Status = ParkingLotStatus.Active;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
