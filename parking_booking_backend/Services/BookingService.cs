using System.Data;
using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Data;
using parking_booking_backend.DTOs;
using parking_booking_backend.Exceptions;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;
using RedLockNet;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace parking_booking_backend.Services;

public sealed class BookingService : IBookingService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IDistributedLockFactory? _lockFactory;
    private readonly IConfiguration _configuration;

    public BookingService(ApplicationDbContext dbContext, ICurrentUserService currentUser, IDistributedLockFactory lockFactory, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _lockFactory = lockFactory;
        _configuration = configuration;
    }

    // Lightweight constructor for isolated service tests. Production DI always uses the full constructor above.
    public BookingService(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "integration-test-jwt-key-at-least-32-characters",
            ["Jwt:Issuer"] = "ParkingBookingBackend",
            ["Jwt:Audience"] = "ParkingBookingClient"
        }).Build();
    }

    public async Task<IReadOnlyCollection<BookingHistoryResponse>> GetMineAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        return await _dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == userId)
            .OrderByDescending(booking => booking.BookingTimestamp)
            .Select(booking => new BookingHistoryResponse(
                booking.Id,
                booking.BookingCode,
                booking.ParkingLot!.Name,
                booking.ParkingLot.Address,
                booking.Vehicle != null ? booking.Vehicle.LicensePlate : booking.GuestLicensePlate ?? "N/A",
                booking.BookingTimestamp,
                booking.BookingTimestamp.AddMinutes(10),
                booking.CheckInTimestamp,
                booking.CheckOutTimestamp,
                booking.Status,
                booking.TotalPrice,
                booking.Voucher != null ? booking.Voucher.Code : null))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ParkingLotSummaryResponse>> GetRecentCompletedParkingLotsAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var recentLotIds = await _dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == userId && booking.Status == BookingStatus.Completed)
            .GroupBy(booking => booking.ParkingLotId)
            .Select(group => new { ParkingLotId = group.Key, LastVisit = group.Max(booking => booking.CheckOutTimestamp ?? booking.BookingTimestamp) })
            .OrderByDescending(item => item.LastVisit)
            .Take(6)
            .ToListAsync(cancellationToken);

        if (recentLotIds.Count == 0) return Array.Empty<ParkingLotSummaryResponse>();

        var ids = recentLotIds.Select(item => item.ParkingLotId).ToList();
        var parkingLots = await _dbContext.ParkingLots
            .AsNoTracking()
            .Where(lot => ids.Contains(lot.Id) && lot.Status == ParkingLotStatus.Active)
            .Select(lot => new ParkingLotSummaryResponse(
                lot.Id, lot.Name, lot.Address, lot.Location.Y, lot.Location.X,
                lot.TotalSlots, lot.AvailableSlots, lot.FirstBlockPrice, lot.FirstBlockHours,
                lot.MaxHeight, lot.HasRoof, lot.Is24_7, lot.AverageRating, lot.Status, null))
            .ToListAsync(cancellationToken);

        return parkingLots.OrderBy(lot => ids.IndexOf(lot.Id)).ToList();
    }

    public async Task<PaginationResponse<StaffBookingResponse>> GetForCurrentStaffAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _dbContext.Bookings
            .AsNoTracking()
            .Where(booking => _dbContext.ParkingLotStaffs.Any(staff =>
                staff.UserId == userId && staff.ParkingLotId == booking.ParkingLotId));
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var items = await query
            .OrderBy(booking => booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.CheckedIn ? 0 : 1)
            .ThenByDescending(booking => booking.BookingTimestamp)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(booking => new StaffBookingResponse(
                booking.Id,
                booking.BookingCode,
                booking.ParkingLot!.Name,
                booking.ParkingSlot!.ParkingFloor!.FloorName,
                booking.ParkingSlot.SlotName,
                booking.Vehicle != null ? booking.Vehicle.LicensePlate : booking.GuestLicensePlate ?? "N/A",
                booking.BookingTimestamp,
                booking.CheckInTimestamp,
                booking.CheckOutTimestamp,
                booking.Status,
                booking.TotalPrice))
            .ToListAsync(cancellationToken);

        return new PaginationResponse<StaffBookingResponse>(items, totalCount, pageIndex, pageSize, totalPages);
    }

    public async Task<PaginationResponse<StaffBookingResponse>> GetForOwnerAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        pageIndex = Math.Max(1, pageIndex);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.ParkingLot != null && booking.ParkingLot.OwnerId == userId);
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        var items = await query
            .OrderBy(booking => booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.CheckedIn ? 0 : 1)
            .ThenByDescending(booking => booking.BookingTimestamp)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(booking => new StaffBookingResponse(
                booking.Id,
                booking.BookingCode,
                booking.ParkingLot!.Name,
                booking.ParkingSlot!.ParkingFloor!.FloorName,
                booking.ParkingSlot.SlotName,
                booking.Vehicle != null ? booking.Vehicle.LicensePlate : booking.GuestLicensePlate ?? "N/A",
                booking.BookingTimestamp,
                booking.CheckInTimestamp,
                booking.CheckOutTimestamp,
                booking.Status,
                booking.TotalPrice))
            .ToListAsync(cancellationToken);

        return new PaginationResponse<StaffBookingResponse>(items, totalCount, pageIndex, pageSize, totalPages);
    }

    public async Task<PaginationResponse<StaffBookingResponse>> GetAllAdminAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Bookings.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(booking => booking.BookingTimestamp)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(booking => new StaffBookingResponse(
                booking.Id,
                booking.BookingCode,
                booking.ParkingLot!.Name,
                booking.ParkingSlot!.ParkingFloor!.FloorName,
                booking.ParkingSlot.SlotName,
                booking.Vehicle != null ? booking.Vehicle.LicensePlate : booking.GuestLicensePlate ?? "N/A",
                booking.BookingTimestamp,
                booking.CheckInTimestamp,
                booking.CheckOutTimestamp,
                booking.Status,
                booking.TotalPrice))
            .ToListAsync(cancellationToken);

        return new PaginationResponse<StaffBookingResponse>(items, totalCount, pageIndex, pageSize, totalPages);
    }

    public async Task<BookingResponse> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await _dbContext.Users.FindAsync(new object[] { _currentUser.UserId }, cancellationToken);
        if (currentUser?.IsLocked == true)
        {
            throw new ApiException("Tài khoản của bạn đã bị khóa do hủy đặt chỗ hoặc không check-in đúng hạn quá 3 lần trong 30 ngày. Không thể tạo đặt chỗ mới.", StatusCodes.Status403Forbidden);
        }

        if (!request.VehicleId.HasValue && string.IsNullOrWhiteSpace(request.GuestLicensePlate))
        {
            throw new ApiException("Either vehicleId or guestLicensePlate is required.");
        }

        if (_lockFactory is null)
        {
            return await CreateUnderSlotLockAsync(request, cancellationToken);
        }

        var userResource = $"booking:user:{_currentUser.UserId}";
        await using var userLock = await _lockFactory.CreateLockAsync(
            userResource,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMilliseconds(200),
            cancellationToken);

        if (!userLock.IsAcquired)
        {
            throw new ApiException("Tài khoản đang thực hiện một yêu cầu đặt chỗ khác. Vui lòng thử lại.", StatusCodes.Status409Conflict);
        }

        var slotResource = $"booking:slot:{request.ParkingSlotId}";
        await using var slotLock = await _lockFactory.CreateLockAsync(
            slotResource,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromMilliseconds(200),
            cancellationToken);

        if (!slotLock.IsAcquired)
        {
            throw new ApiException("Chỗ đỗ này đang được người khác thao tác. Vui lòng thử lại sau giây lát.", StatusCodes.Status409Conflict);
        }

        return await CreateUnderSlotLockAsync(request, cancellationToken);
    }

    private async Task<BookingResponse> CreateUnderSlotLockAsync(CreateBookingRequest request, CancellationToken cancellationToken)
    {

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var activeBooking = await _dbContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == _currentUser.UserId
                && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.CheckedIn))
            .OrderByDescending(booking => booking.BookingTimestamp)
            .Select(booking => new { booking.Id })
            .FirstOrDefaultAsync(cancellationToken);
        if (activeBooking is not null)
        {
            throw new ApiException(
                "Bạn đang có một lượt đặt chỗ chưa hoàn tất. Vui lòng hủy hoặc checkout lượt hiện tại trước khi đặt mới.",
                StatusCodes.Status409Conflict,
                "ACTIVE_BOOKING_EXISTS",
                activeBooking.Id);
        }

        var slot = await _dbContext.ParkingSlots
            .Include(s => s.ParkingFloor)
            .FirstOrDefaultAsync(s => s.Id == request.ParkingSlotId, cancellationToken)
            ?? throw new ApiException("Parking slot was not found.", StatusCodes.Status404NotFound);

        if (slot.Status != ParkingSlotStatus.Available)
        {
            throw new ApiException("Parking slot is not available.", StatusCodes.Status409Conflict);
        }

        var hasActiveBooking = await _dbContext.Bookings.AnyAsync(b =>
            b.ParkingSlotId == request.ParkingSlotId
            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.CheckedIn),
            cancellationToken);

        if (hasActiveBooking)
        {
            throw new ApiException("Parking slot already has an active booking.", StatusCodes.Status409Conflict);
        }

        if (request.VehicleId.HasValue)
        {
            var ownsVehicle = await _dbContext.Vehicles.AnyAsync(v => v.Id == request.VehicleId.Value && v.UserId == _currentUser.UserId, cancellationToken);
            if (!ownsVehicle)
            {
                throw new ApiException("Vehicle was not found.", StatusCodes.Status404NotFound);
            }
        }

        if (slot.ParkingFloor is null)
        {
            throw new ApiException("Parking slot is not assigned to a floor.", StatusCodes.Status409Conflict);
        }

        var parkingLot = await _dbContext.ParkingLots
            .FirstAsync(p => p.Id == slot.ParkingFloor.ParkingLotId, cancellationToken);

        if (parkingLot.AvailableSlots <= 0)
        {
            throw new ApiException("Parking lot is full.", StatusCodes.Status409Conflict);
        }

        slot.Status = ParkingSlotStatus.Occupied;
        parkingLot.AvailableSlots -= 1;

        var booking = new Booking
        {
            UserId = request.VehicleId.HasValue ? _currentUser.UserId : null,
            VehicleId = request.VehicleId,
            GuestLicensePlate = request.GuestLicensePlate?.Trim().ToUpperInvariant(),
            ParkingLotId = parkingLot.Id,
            ParkingSlotId = slot.Id,
            BookingCode = CreateBookingCode(),
            BookingTimestamp = DateTime.UtcNow,
            Status = BookingStatus.Pending
        };

        _dbContext.Bookings.Add(booking);
        if (booking.UserId.HasValue)
        {
            AddNotification(booking.UserId.Value, "Đặt chỗ thành công",
                $"Mã {booking.BookingCode} đã được giữ. Vui lòng check-in trước {BookingPolicy.FormatVietnamTime(BookingPolicy.GetCheckInDeadline(booking.BookingTimestamp))}.");
        }

        AddNotification(parkingLot.OwnerId, "Có lượt đặt chỗ mới",
            $"Lượt {booking.BookingCode} vừa đặt chỗ tại {parkingLot.Name}.");
        var staffUserIds = await _dbContext.ParkingLotStaffs
            .Where(item => item.ParkingLotId == parkingLot.Id)
            .Select(item => item.UserId)
            .ToListAsync(cancellationToken);
        foreach (var staffUserId in staffUserIds)
        {
            AddNotification(staffUserId, "Có xe sắp đến", $"Lượt {booking.BookingCode} đang chờ check-in tại bãi của bạn.");
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToResponse(booking);
    }

    public async Task<BookingResponse> CheckInAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await GetBookingForCurrentUserAsync(id, cancellationToken);

        if (booking.Status != BookingStatus.Pending)
        {
            throw new ApiException("Only pending bookings can be checked in.", StatusCodes.Status409Conflict);
        }

        booking.Status = BookingStatus.CheckedIn;
        booking.CheckInTimestamp = DateTime.UtcNow;
        if (booking.UserId.HasValue)
        {
            AddNotification(booking.UserId.Value, "Check-in thành công", $"Lượt {booking.BookingCode} đã được check-in.");
        }
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(booking);
    }

    public async Task<BookingResponse> ApplyVoucherAsync(Guid id, ApplyVoucherRequest request, CancellationToken cancellationToken)
    {
        var booking = await GetBookingForCurrentUserAsync(id, cancellationToken);

        if (booking.Status is BookingStatus.Completed or BookingStatus.Cancelled or BookingStatus.NoShow)
        {
            throw new ApiException("Voucher cannot be applied to a closed booking.", StatusCodes.Status409Conflict);
        }

        var code = request.VoucherCode.Trim().ToUpperInvariant();
        var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(v => v.Code == code, cancellationToken)
            ?? throw new ApiException("Voucher was not found.", StatusCodes.Status404NotFound);

        if (voucher.ExpiryDate < DateTime.UtcNow || voucher.UsageLimit <= 0)
        {
            throw new ApiException("Voucher is expired or unavailable.", StatusCodes.Status409Conflict);
        }

        booking.VoucherId = voucher.Id;
        booking.Voucher = voucher;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(booking);
    }

    public async Task<CheckOutResponse> CheckOutAsync(Guid id, CheckOutRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var booking = await GetBookingForCurrentUserAsync(id, cancellationToken);
        if (booking.Status != BookingStatus.CheckedIn)
        {
            throw new ApiException("Only checked-in bookings can be checked out.", StatusCodes.Status409Conflict);
        }

        var parkingLot = await _dbContext.ParkingLots.FirstAsync(p => p.Id == booking.ParkingLotId, cancellationToken);
        var slot = await _dbContext.ParkingSlots.FirstAsync(s => s.Id == booking.ParkingSlotId, cancellationToken);
        var totalPrice = CalculatePrice(parkingLot, booking.CheckInTimestamp ?? DateTime.UtcNow, DateTime.UtcNow);

        if (booking.VoucherId.HasValue)
        {
            var voucher = await _dbContext.Vouchers.FirstOrDefaultAsync(v => v.Id == booking.VoucherId.Value, cancellationToken);
            if (voucher is not null)
            {
                totalPrice = ApplyDiscount(totalPrice, voucher);
                voucher.UsageLimit = Math.Max(0, voucher.UsageLimit - 1);
            }
        }

        booking.CheckOutTimestamp = DateTime.UtcNow;
        booking.TotalPrice = totalPrice;
        booking.Status = BookingStatus.Completed;
        slot.Status = ParkingSlotStatus.Available;
        parkingLot.AvailableSlots = Math.Min(parkingLot.TotalSlots, parkingLot.AvailableSlots + 1);

        var paymentStatus = request.CollectCash ? TransactionStatus.Success : TransactionStatus.Pending;
        string? vietQrUrl = request.CollectCash ? null : CreateVietQrUrl(booking.BookingCode, totalPrice);

        if (request.UseWallet)
        {
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == _currentUser.UserId, cancellationToken)
                ?? throw new ApiException("Wallet was not found.", StatusCodes.Status404NotFound);

            if (wallet.Balance < totalPrice)
            {
                throw new ApiException("Wallet balance is not enough.", StatusCodes.Status409Conflict);
            }

            wallet.Balance -= totalPrice;
            _dbContext.WalletTransactions.Add(new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = -totalPrice,
                Type = WalletTransactionType.Payment,
                ReferenceId = booking.Id.ToString()
            });

            paymentStatus = TransactionStatus.Success;
            vietQrUrl = null;
        }

        _dbContext.Transactions.Add(new Transaction
        {
            BookingId = booking.Id,
            Amount = totalPrice,
            PaymentMethod = request.UseWallet ? PaymentMethod.Wallet : request.CollectCash ? PaymentMethod.Cash : PaymentMethod.VietQR,
            Status = paymentStatus,
            TransactionDate = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new CheckOutResponse(totalPrice, vietQrUrl, paymentStatus, booking.CheckOutTimestamp.Value);
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var booking = await GetBookingForCurrentUserAsync(id, cancellationToken);
        if (booking.Status != BookingStatus.Pending)
        {
            throw new ApiException("Only pending bookings can be cancelled.", StatusCodes.Status409Conflict);
        }

        var parkingLot = await _dbContext.ParkingLots.FirstAsync(p => p.Id == booking.ParkingLotId, cancellationToken);
        var slot = await _dbContext.ParkingSlots.FirstAsync(s => s.Id == booking.ParkingSlotId, cancellationToken);

        booking.Status = BookingStatus.Cancelled;
        slot.Status = ParkingSlotStatus.Available;
        parkingLot.AvailableSlots = Math.Min(parkingLot.TotalSlots, parkingLot.AvailableSlots + 1);
        if (booking.UserId.HasValue)
        {
            AddNotification(booking.UserId.Value, "Đã hủy đặt chỗ", $"Lượt {booking.BookingCode} đã được hủy.");
        }

        if (booking.UserId.HasValue && await ShouldLockUserAfterViolationAsync(booking, cancellationToken))
        {
            var user = await _dbContext.Users.FindAsync(new object?[] { booking.UserId }, cancellationToken);
            if (user != null)
            {
                user.IsLocked = true;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task MarkNoShowAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var booking = await _dbContext.Bookings.FindAsync(new object?[] { id }, cancellationToken);
        if (booking == null) throw new ApiException("Booking not found", StatusCodes.Status404NotFound);

        if (booking.Status != BookingStatus.Pending)
        {
            throw new ApiException("Chỉ có thể đánh dấu Không đến cho những lượt đặt chỗ đang chờ.", StatusCodes.Status409Conflict);
        }

        var parkingLot = await _dbContext.ParkingLots.FirstAsync(p => p.Id == booking.ParkingLotId, cancellationToken);
        var slot = await _dbContext.ParkingSlots.FirstAsync(s => s.Id == booking.ParkingSlotId, cancellationToken);

        booking.Status = BookingStatus.NoShow;
        slot.Status = ParkingSlotStatus.Available;
        parkingLot.AvailableSlots = Math.Min(parkingLot.TotalSlots, parkingLot.AvailableSlots + 1);
        if (booking.UserId.HasValue)
        {
            AddNotification(booking.UserId.Value, "Không check-in đúng hạn",
                $"Lượt {booking.BookingCode} được ghi nhận là không đến và tính vào chính sách vi phạm.");
        }

        if (booking.UserId.HasValue && await ShouldLockUserAfterViolationAsync(booking, cancellationToken))
        {
            var user = await _dbContext.Users.FindAsync(new object?[] { booking.UserId }, cancellationToken);
            if (user != null)
            {
                user.IsLocked = true;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<BookingQrResponse> GenerateQrTokenAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await GetBookingForCurrentUserAsync(id, cancellationToken);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, booking.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, booking.Id.ToString()),
            new Claim("purpose", "booking_qr")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new BookingQrResponse(tokenHandler.WriteToken(token));
    }

    public async Task<VerifyQrResponse> VerifyQrTokenAsync(VerifyQrRequest request, CancellationToken cancellationToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        try
        {
            var principal = tokenHandler.ValidateToken(request.QrToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var purpose = principal.FindFirst("purpose")?.Value;
            if (purpose != "booking_qr")
            {
                return new VerifyQrResponse(false, null, null, null, "Invalid QR purpose.");
            }

            var bookingIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(bookingIdStr) || !Guid.TryParse(bookingIdStr, out var bookingId))
            {
                return new VerifyQrResponse(false, null, null, null, "Invalid booking ID in QR.");
            }

            var booking = await _dbContext.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

            if (booking == null)
            {
                return new VerifyQrResponse(false, null, null, null, "Booking not found.");
            }

            var canManageParkingLot = await _dbContext.ParkingLotStaffs.AnyAsync(
                staff => staff.ParkingLotId == booking.ParkingLotId && staff.UserId == _currentUser.UserId,
                cancellationToken);
            if (!canManageParkingLot)
            {
                return new VerifyQrResponse(false, null, null, null, "Bạn không được phân công tại bãi xe của booking này.");
            }

            var licensePlate = booking.Vehicle != null ? booking.Vehicle.LicensePlate : booking.GuestLicensePlate;
            return new VerifyQrResponse(true, booking.Id, booking.BookingCode, licensePlate, "QR Verified successfully.");
        }
        catch (Exception ex)
        {
            return new VerifyQrResponse(false, null, null, null, "Invalid QR Code: " + ex.Message);
        }
    }

    public async Task<ProcessQrResponse> ProcessQrAsync(VerifyQrRequest request, CancellationToken cancellationToken)
    {
        var bookingId = ValidateBookingQrToken(request.QrToken);
        var booking = await _dbContext.Bookings
            .Include(item => item.Vehicle)
            .Include(item => item.ParkingLot)
            .Include(item => item.Voucher)
            .FirstOrDefaultAsync(item => item.Id == bookingId, cancellationToken)
            ?? throw new ApiException("Không tìm thấy booking trong mã QR.", StatusCodes.Status404NotFound);

        var canManageParkingLot = await _dbContext.ParkingLotStaffs.AnyAsync(
            staff => staff.ParkingLotId == booking.ParkingLotId && staff.UserId == _currentUser.UserId,
            cancellationToken);
        if (!canManageParkingLot)
        {
            throw new ApiException("Bạn không được phân công tại bãi xe của booking này.", StatusCodes.Status403Forbidden);
        }

        var licensePlate = booking.Vehicle?.LicensePlate ?? booking.GuestLicensePlate ?? "N/A";
        if (booking.Status == BookingStatus.Pending)
        {
            booking.Status = BookingStatus.CheckedIn;
            booking.CheckInTimestamp = DateTime.UtcNow;
            if (booking.UserId.HasValue)
            {
                AddNotification(booking.UserId.Value, "Check-in thành công", $"Lượt {booking.BookingCode} đã được check-in.");
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new ProcessQrResponse(booking.Id, booking.BookingCode, licensePlate, booking.ParkingLotId,
                booking.ParkingLot!.Name, booking.Status, "CheckedIn", null, booking.CheckInTimestamp);
        }

        if (booking.Status == BookingStatus.CheckedIn)
        {
            var estimatedTotal = CalculatePrice(booking.ParkingLot!, booking.CheckInTimestamp ?? DateTime.UtcNow, DateTime.UtcNow);
            if (booking.Voucher is not null)
            {
                estimatedTotal = ApplyDiscount(estimatedTotal, booking.Voucher);
            }
            return new ProcessQrResponse(booking.Id, booking.BookingCode, licensePlate, booking.ParkingLotId,
                booking.ParkingLot!.Name, booking.Status, "CheckoutConfirmationRequired", estimatedTotal, booking.CheckInTimestamp);
        }

        throw new ApiException($"Booking đang ở trạng thái {booking.Status} và không thể xử lý lại bằng QR.", StatusCodes.Status409Conflict);
    }

    private Guid ValidateBookingQrToken(string qrToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        try
        {
            var principal = tokenHandler.ValidateToken(qrToken, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            if (principal.FindFirst("purpose")?.Value != "booking_qr")
            {
                throw new ApiException("Mã QR không đúng mục đích booking.", StatusCodes.Status400BadRequest);
            }
            var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(id, out var bookingId))
            {
                throw new ApiException("Mã QR không chứa booking hợp lệ.", StatusCodes.Status400BadRequest);
            }
            return bookingId;
        }
        catch (ApiException)
        {
            throw;
        }
        catch
        {
            throw new ApiException("Mã QR không hợp lệ hoặc đã hết hạn.", StatusCodes.Status400BadRequest);
        }
    }

    private async Task<Booking> GetBookingForCurrentUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _dbContext.Bookings.Include(b => b.Voucher).FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new ApiException("Booking was not found.", StatusCodes.Status404NotFound);

        var isBookingOwner = booking.UserId == _currentUser.UserId;
        var canManageParkingLot = await _dbContext.ParkingLotStaffs.AnyAsync(
            staff => staff.ParkingLotId == booking.ParkingLotId && staff.UserId == _currentUser.UserId,
            cancellationToken);
        var ownsParkingLot = await _dbContext.ParkingLots.AnyAsync(
            parkingLot => parkingLot.Id == booking.ParkingLotId && parkingLot.OwnerId == _currentUser.UserId,
            cancellationToken);

        if (!isBookingOwner && !canManageParkingLot && !ownsParkingLot)
        {
            throw new ApiException("You cannot access this booking.", StatusCodes.Status403Forbidden);
        }

        return booking;
    }

    private static decimal CalculatePrice(ParkingLot parkingLot, DateTime checkIn, DateTime checkOut)
    {
        var hours = Math.Max(1, (int)Math.Ceiling((checkOut - checkIn).TotalHours));
        var blocks = Math.Max(1, (int)Math.Ceiling(hours / (double)Math.Max(1, parkingLot.FirstBlockHours)));
        return parkingLot.FirstBlockPrice * blocks;
    }

    private static decimal ApplyDiscount(decimal totalPrice, Voucher voucher)
    {
        var discount = voucher.DiscountAmount ?? 0;
        if (voucher.DiscountPercentage.HasValue)
        {
            var percentageDiscount = totalPrice * (decimal)voucher.DiscountPercentage.Value / 100;
            discount = voucher.MaxDiscount.HasValue
                ? Math.Min(percentageDiscount, voucher.MaxDiscount.Value)
                : percentageDiscount;
        }

        return Math.Max(0, totalPrice - discount);
    }

    private static string CreateBookingCode()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", string.Empty, StringComparison.Ordinal)
            .Replace("/", string.Empty, StringComparison.Ordinal)
            .Replace("=", string.Empty, StringComparison.Ordinal)
            .ToUpperInvariant()[..6];
    }

    private static string CreateVietQrUrl(string bookingCode, decimal amount)
        => $"https://img.vietqr.io/image/demo-bank-demo-account-compact2.png?amount={amount:0}&addInfo=PKB%20{bookingCode}";

    private async Task<bool> ShouldLockUserAfterViolationAsync(Booking currentBooking, CancellationToken cancellationToken)
    {
        if (!currentBooking.UserId.HasValue)
        {
            return false;
        }

        var windowStart = DateTime.UtcNow.Subtract(BookingPolicy.ViolationWindow);
        var previousViolationCount = await _dbContext.Bookings
            .Where(booking => booking.Id != currentBooking.Id
                && booking.UserId == currentBooking.UserId
                && booking.BookingTimestamp >= windowStart
                && (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.NoShow))
            .CountAsync(cancellationToken);

        return BookingPolicy.ShouldLock(previousViolationCount + 1);
    }

    private void AddNotification(Guid userId, string title, string message)
        => _dbContext.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            IsRead = false
        });

    private static BookingResponse ToResponse(Booking booking)
        => new(
            booking.Id,
            booking.UserId,
            booking.VehicleId,
            booking.GuestLicensePlate,
            booking.ParkingLotId,
            booking.ParkingSlotId,
            booking.BookingCode,
            booking.BookingTimestamp,
            BookingPolicy.GetCheckInDeadline(booking.BookingTimestamp),
            booking.CheckInTimestamp,
            booking.CheckOutTimestamp,
            booking.Status,
            booking.TotalPrice,
            booking.Voucher?.Code);
}
