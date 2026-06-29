using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Services;

public sealed class MonthlyPassService : IMonthlyPassService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public MonthlyPassService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<MonthlyPassResponse>> GetMineAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.MonthlyPasses
            .AsNoTracking()
            .Where(p => p.UserId == _currentUser.UserId && p.Status == MonthlyPassStatus.Active)
            .OrderBy(p => p.EndDate)
            .Select(p => ToResponse(p, null))
            .ToListAsync(cancellationToken);
    }

    public async Task<MonthlyPassResponse> CreateAsync(CreateMonthlyPassRequest request, CancellationToken cancellationToken)
    {
        var ownsVehicle = await _dbContext.Vehicles.AnyAsync(v => v.Id == request.VehicleId && v.UserId == _currentUser.UserId, cancellationToken);
        if (!ownsVehicle)
        {
            throw new ApiException("Vehicle was not found.", StatusCodes.Status404NotFound);
        }

        var parkingLot = await _dbContext.ParkingLots.FirstOrDefaultAsync(p => p.Id == request.ParkingLotId, cancellationToken)
            ?? throw new ApiException("Parking lot was not found.", StatusCodes.Status404NotFound);

        var startDate = DateTime.UtcNow;
        var price = parkingLot.FirstBlockPrice * request.DurationDays;
        
        string? vietQrUrl = null;
        var status = MonthlyPassStatus.PendingPayment;

        if (request.UseWallet)
        {
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == _currentUser.UserId, cancellationToken)
                ?? throw new ApiException("Wallet was not found.", StatusCodes.Status404NotFound);

            if (wallet.Balance < price)
            {
                throw new ApiException("Wallet balance is not enough.", StatusCodes.Status409Conflict);
            }

            wallet.Balance -= price;
            _dbContext.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = -price,
                Type = WalletTransactionType.Payment,
                ReferenceId = $"MP_{request.VehicleId}_{startDate.Ticks}"
            });

            status = MonthlyPassStatus.Active;
        }
        else
        {
            vietQrUrl = $"https://img.vietqr.io/image/demo-bank-demo-account-compact2.png?amount={price:0}&addInfo=MP%20{_currentUser.UserId}";
        }

        var monthlyPass = new MonthlyPass
        {
            UserId = _currentUser.UserId,
            VehicleId = request.VehicleId,
            ParkingLotId = request.ParkingLotId,
            StartDate = startDate,
            EndDate = startDate.AddDays(request.DurationDays),
            Price = price,
            Status = status
        };

        _dbContext.MonthlyPasses.Add(monthlyPass);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return ToResponse(monthlyPass, vietQrUrl);
    }

    private static MonthlyPassResponse ToResponse(MonthlyPass monthlyPass, string? paymentUrl = null)
    {
        return new MonthlyPassResponse(
            monthlyPass.Id,
            monthlyPass.UserId,
            monthlyPass.VehicleId,
            monthlyPass.ParkingLotId,
            monthlyPass.StartDate,
            monthlyPass.EndDate,
            monthlyPass.Price,
            monthlyPass.Status,
            paymentUrl);
    }
}
