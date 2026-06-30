using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class FavouriteParkingLotService(ApplicationDbContext dbContext, ICurrentUserService currentUser) : IFavouriteParkingLotService
{
    public async Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetMineAsync(CancellationToken cancellationToken)
        => await dbContext.FavouriteParkingLots
            .AsNoTracking()
            .Where(item => item.UserId == currentUser.UserId)
            .OrderBy(item => item.ParkingLot!.Name)
            .Select(item => new ParkingLotSummaryResponse(
                item.ParkingLotId, item.ParkingLot!.Name, item.ParkingLot.Address,
                item.ParkingLot.Location.Y, item.ParkingLot.Location.X,
                item.ParkingLot.TotalSlots, item.ParkingLot.AvailableSlots,
                item.ParkingLot.FirstBlockPrice, item.ParkingLot.FirstBlockHours,
                item.ParkingLot.MaxHeight, item.ParkingLot.HasRoof, item.ParkingLot.Is24_7,
                item.ParkingLot.AverageRating, item.ParkingLot.Status, null))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Guid parkingLotId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.ParkingLots.AnyAsync(
            lot => lot.Id == parkingLotId && lot.Status == ParkingLotStatus.Active,
            cancellationToken);
        if (!exists)
        {
            throw new ApiException("Không tìm thấy bãi đỗ đang hoạt động.", StatusCodes.Status404NotFound);
        }

        if (!await dbContext.FavouriteParkingLots.AnyAsync(
                item => item.UserId == currentUser.UserId && item.ParkingLotId == parkingLotId,
                cancellationToken))
        {
            dbContext.FavouriteParkingLots.Add(new FavouriteParkingLot
            {
                UserId = currentUser.UserId,
                ParkingLotId = parkingLotId
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveAsync(Guid parkingLotId, CancellationToken cancellationToken)
    {
        var favourite = await dbContext.FavouriteParkingLots.FirstOrDefaultAsync(
            item => item.UserId == currentUser.UserId && item.ParkingLotId == parkingLotId,
            cancellationToken) ?? throw new ApiException("Bãi đỗ chưa nằm trong danh sách yêu thích.", StatusCodes.Status404NotFound);
        dbContext.FavouriteParkingLots.Remove(favourite);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
