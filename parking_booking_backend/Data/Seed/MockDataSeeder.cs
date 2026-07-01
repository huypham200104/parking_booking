using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using parking_booking_backend.Interfaces;
using parking_booking_backend.Models;

namespace parking_booking_backend.Data.Seed;

public sealed class MockDataSeeder : IMockDataSeeder
{
    private const string SeedMarkerPhoneNumber = "0900000000";
    private readonly ApplicationDbContext _dbContext;
    private readonly GeometryFactory _geometryFactory = new(new PrecisionModel(), 4326);

    public MockDataSeeder(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SeedResult> SeedAsync(bool recreateDatabase, CancellationToken cancellationToken)
    {
        if (recreateDatabase)
        {
            await _dbContext.Database.EnsureDeletedAsync(cancellationToken);
        }

        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var alreadySeeded = await _dbContext.Users.AnyAsync(u => u.PhoneNumber == SeedMarkerPhoneNumber, cancellationToken);
        if (alreadySeeded)
        {
            return new SeedResult(false, 0, 0, 0, 0, 0, 0, "Mock data already exists.");
        }

        var users = CreateUsers();
        var wallets = users.Select(user => new Wallet
        {
            Id = DeterministicGuid($"wallet-{user.Id}"),
            UserId = user.Id,
            Balance = user.Role == Role.Driver ? 500_000 : 0
        }).ToList();

        var vehicles = CreateVehicles(users);
        var templates = CreateLayoutTemplates();
        var parkingLots = CreateParkingLots(users);
        var floors = CreateParkingFloors(parkingLots, templates);
        var slots = CreateParkingSlots(floors);
        var vouchers = CreateVouchers();
        var staff = CreateStaffAssignments(users, parkingLots);
        var favourites = CreateFavouriteParkingLots(users, parkingLots);
        var notifications = CreateNotifications(users);
        var bankAccounts = CreateBankAccounts(users);
        var monthlyPasses = CreateMonthlyPasses(users, vehicles, parkingLots);
        var bookings = CreateBookings(users, vehicles, parkingLots, slots, vouchers);
        var demoExtraBookings = CreateDemoUserRichData(users, vehicles, parkingLots, slots, vouchers);
        bookings.AddRange(demoExtraBookings);
        var transactions = CreateTransactions(bookings);
        var reviews = CreateReviews(users, parkingLots, bookings);
        var reports = CreateCrowdsourceReports(users, parkingLots);

        _dbContext.Users.AddRange(users);
        _dbContext.Wallets.AddRange(wallets);
        _dbContext.Vehicles.AddRange(vehicles);
        _dbContext.LayoutTemplates.AddRange(templates);
        _dbContext.ParkingLots.AddRange(parkingLots);
        _dbContext.ParkingFloors.AddRange(floors);
        _dbContext.ParkingSlots.AddRange(slots);
        _dbContext.Vouchers.AddRange(vouchers);
        _dbContext.ParkingLotStaffs.AddRange(staff);
        _dbContext.FavouriteParkingLots.AddRange(favourites);
        _dbContext.Notifications.AddRange(notifications);
        _dbContext.BankAccounts.AddRange(bankAccounts);
        _dbContext.MonthlyPasses.AddRange(monthlyPasses);
        _dbContext.Bookings.AddRange(bookings);
        _dbContext.Transactions.AddRange(transactions);
        _dbContext.Reviews.AddRange(reviews);
        _dbContext.CrowdsourceReports.AddRange(reports);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new SeedResult(
            true,
            users.Count,
            parkingLots.Count,
            floors.Count,
            slots.Count,
            vehicles.Count,
            vouchers.Count,
            "Mock data has been created.");
    }

    private static List<User> CreateUsers()
    {
        var users = new List<User>
        {
            new User { Id = DeterministicGuid("user-admin"), PhoneNumber = SeedMarkerPhoneNumber, FullName = "System Admin", Role = Role.Admin },
            new User { Id = DeterministicGuid("user-owner-1"), PhoneNumber = "0911000001", FullName = "Nguyen Minh Quan", Role = Role.ParkingOwner },
            new User { Id = DeterministicGuid("user-owner-2"), PhoneNumber = "0911000002", FullName = "Tran Hoang Anh", Role = Role.ParkingOwner },
            new User { Id = DeterministicGuid("user-owner-3"), PhoneNumber = "0911000003", FullName = "Pham Gia Phuc", Role = Role.ParkingOwner },
            new User { Id = DeterministicGuid("user-owner-4"), PhoneNumber = "0911000004", FullName = "Le Minh Chau", Role = Role.ParkingOwner },
            new User { Id = DeterministicGuid("user-owner-5"), PhoneNumber = "0911000005", FullName = "Vo Thanh Dat", Role = Role.ParkingOwner }
        };

        for (int i = 1; i <= 60; i++)
        {
            users.Add(new User { Id = DeterministicGuid($"user-guard-{i}"), PhoneNumber = $"0922{i:D6}", FullName = $"Bảo vệ {i}", Role = Role.Guard });
        }

        for (int i = 1; i <= 30; i++)
        {
            users.Add(new User { Id = DeterministicGuid($"user-driver-{i}"), PhoneNumber = $"0933{i:D6}", FullName = $"Tài xế {i}", Role = Role.Driver, TrustScore = 100 });
        }

        for (int i = 31; i <= 35; i++)
        {
            users.Add(new User { Id = DeterministicGuid($"user-driver-{i}"), PhoneNumber = $"0933{i:D6}", FullName = $"Tài xế vi phạm {i}", Role = Role.Driver, TrustScore = 40, IsLocked = true });
        }

        return users;
    }

    private static List<Vehicle> CreateVehicles(IReadOnlyCollection<User> users)
    {
        var drivers = users
            .Where(u => u.Role == Role.Driver)
            .OrderBy(u => u.PhoneNumber)
            .ToList();

        var vehicleTypes = new[] { VehicleType.Sedan, VehicleType.SUV, VehicleType.Hatchback };

        return drivers
            .Select((driver, index) => new Vehicle
            {
                Id = DeterministicGuid($"vehicle-driver-{index + 1}-main"),
                UserId = driver.Id,
                LicensePlate = $"51{(char)('F' + index % 8)}-{120 + index:000}.{45 + index:00}",
                VehicleType = vehicleTypes[index % vehicleTypes.Length],
                IsDefault = true
            })
            .ToList();
    }

    private static List<LayoutTemplate> CreateLayoutTemplates()
    {
        return
        [
            new LayoutTemplate { Id = DeterministicGuid("template-basement-grid"), Name = "Basement Grid 24", ImageUrl = "https://example.local/layouts/basement-grid-24.png", Description = "Standard basement parking grid with two driving lanes." },
            new LayoutTemplate { Id = DeterministicGuid("template-outdoor-row"), Name = "Outdoor Row 18", ImageUrl = "https://example.local/layouts/outdoor-row-18.png", Description = "Outdoor parking rows for small private lots." },
            new LayoutTemplate { Id = DeterministicGuid("template-angled"), Name = "Angled Slots 30", ImageUrl = "https://example.local/layouts/angled-slots-30.png", Description = "Angled parking slots for high traffic areas." }
        ];
    }

    private List<ParkingLot> CreateParkingLots(IReadOnlyCollection<User> users)
    {
        var owners = users
            .Where(u => u.Role == Role.ParkingOwner)
            .OrderBy(u => u.PhoneNumber)
            .ToList();

        return
        [
            CreateParkingLot("parking-vincom-dong-khoi", owners[0].Id, "Bãi xe Vincom Đồng Khởi", "72 Lê Thánh Tôn, Bến Nghé, Quận 1", 10.77880, 106.70170, 36, 28, 25_000, 1, 2.1m, true, true, 4.6f),
            CreateParkingLot("parking-saigon-centre", owners[0].Id, "Bãi xe Saigon Centre", "65 Lê Lợi, Bến Nghé, Quận 1", 10.77390, 106.70030, 40, 31, 30_000, 1, 2.0m, true, true, 4.7f),
            CreateParkingLot("parking-ben-thanh", owners[1].Id, "Bãi xe Chợ Bến Thành", "Đường Lê Lai, Bến Thành, Quận 1", 10.77220, 106.69810, 24, 16, 20_000, 1, null, false, false, 4.2f, new TimeSpan(6, 0, 0), new TimeSpan(22, 0, 0)),
            CreateParkingLot("parking-nguyen-hue", owners[1].Id, "Bãi đỗ Phố đi bộ Nguyễn Huệ", "Phố đi bộ Nguyễn Huệ, Quận 1", 10.77520, 106.70370, 18, 8, 15_000, 1, null, false, false, 4.0f, new TimeSpan(7, 0, 0), new TimeSpan(23, 0, 0)),
            CreateParkingLot("parking-tao-dan", owners[0].Id, "Bãi xe Công viên Tao Đàn", "Đường Trương Định, Quận 1", 10.77470, 106.69390, 30, 22, 18_000, 1, null, true, true, 4.3f),
            CreateParkingLot("parking-diamond-plaza", owners[2].Id, "Bãi xe Diamond Plaza", "34 Lê Duẩn, Bến Nghé, Quận 1", 10.78210, 106.69840, 32, 21, 28_000, 1, 2.05m, true, true, 4.4f),
            CreateParkingLot("parking-notre-dame", owners[2].Id, "Bãi xe Nhà thờ Đức Bà", "Công xã Paris, Bến Nghé, Quận 1", 10.77990, 106.69910, 22, 14, 22_000, 1, 2.0m, true, false, 4.1f, new TimeSpan(6, 30, 0), new TimeSpan(22, 30, 0)),
            CreateParkingLot("parking-takashimaya", owners[3].Id, "Hầm xe Takashimaya", "92 Nam Kỳ Khởi Nghĩa, Bến Nghé, Quận 1", 10.77330, 106.70080, 42, 35, 30_000, 1, 2.0m, true, true, 4.8f),
            CreateParkingLot("parking-le-loi", owners[3].Id, "Bãi đỗ đường Lê Lợi", "Đại lộ Lê Lợi, Quận 1", 10.77370, 106.69920, 16, 7, 15_000, 1, null, false, false, 3.9f, new TimeSpan(7, 0, 0), new TimeSpan(22, 0, 0)),
            CreateParkingLot("parking-ham-nghi", owners[4].Id, "Bãi đỗ đường Hàm Nghi", "Đại lộ Hàm Nghi, Quận 1", 10.77110, 106.70410, 16, 9, 15_000, 1, null, false, false, 4.0f, new TimeSpan(7, 0, 0), new TimeSpan(22, 0, 0)),
            CreateParkingLot("parking-bitexco", owners[4].Id, "Bãi xe Tòa nhà Bitexco", "2 Hải Triều, Bến Nghé, Quận 1", 10.77170, 106.70490, 38, 25, 32_000, 1, 2.05m, true, true, 4.6f),
            CreateParkingLot("parking-opera-house", owners[1].Id, "Bãi xe Nhà hát Thành phố", "7 Công trường Lam Sơn, Bến Nghé, Quận 1", 10.77660, 106.70300, 20, 13, 24_000, 1, 2.0m, true, false, 4.3f, new TimeSpan(6, 0, 0), new TimeSpan(23, 0, 0)),
            CreateParkingLot("parking-bui-vien", owners[2].Id, "Bãi xe đêm Bùi Viện", "Phường Phạm Ngũ Lão, Quận 1", 10.76840, 106.69300, 28, 11, 18_000, 1, null, false, true, 4.1f),
            CreateParkingLot("parking-september-23", owners[3].Id, "Bãi xe Công viên 23 tháng 9", "Lê Lai, Phạm Ngũ Lão, Quận 1", 10.77040, 106.69460, 34, 24, 18_000, 1, null, true, true, 4.2f),
            CreateParkingLot("parking-independence-palace", owners[4].Id, "Bãi xe Dinh Độc Lập", "135 Nam Kỳ Khởi Nghĩa, Quận 1", 10.77720, 106.69530, 26, 18, 20_000, 1, null, true, false, 4.4f, new TimeSpan(6, 0, 0), new TimeSpan(21, 30, 0)),
            CreateParkingLot("parking-vincom-thu-duc", owners[0].Id, "Bãi xe Vincom Thủ Đức", "216 Võ Văn Ngân, Thủ Đức", 10.85210, 106.75840, 50, 45, 20_000, 1, 2.1m, true, true, 4.5f),
            CreateParkingLot("parking-gigamall", owners[1].Id, "Bãi xe Gigamall Phạm Văn Đồng", "240-242 Phạm Văn Đồng, Thủ Đức", 10.82860, 106.72140, 80, 60, 20_000, 1, 2.2m, true, true, 4.7f),
            CreateParkingLot("parking-coopmart-phu-lam", owners[2].Id, "Bãi xe Co.opmart Phú Lâm", "06 Bà Hom, Quận 6", 10.75230, 106.63180, 40, 25, 15_000, 1, null, true, false, 4.1f, new TimeSpan(7, 30, 0), new TimeSpan(22, 0, 0)),
            CreateParkingLot("parking-mega-market", owners[3].Id, "Bãi xe Mega Market Bình Phú", "Bình Phú, Quận 6", 10.74230, 106.62680, 100, 80, 10_000, 1, null, false, false, 4.0f),
            CreateParkingLot("parking-pending-1", owners[4].Id, "Bãi xe đang chờ duyệt", "Khu công nghệ cao Thủ Đức", 10.85220, 106.79810, 50, 50, 15_000, 1, null, true, true, 0f, null, null, ParkingLotStatus.PendingApproval)
        ];
    }

    private ParkingLot CreateParkingLot(
        string key,
        Guid ownerId,
        string name,
        string address,
        double latitude,
        double longitude,
        int totalSlots,
        int availableSlots,
        decimal firstBlockPrice,
        int firstBlockHours,
        decimal? maxHeight,
        bool hasRoof,
        bool is24_7,
        float rating,
        TimeSpan? openTime = null,
        TimeSpan? closeTime = null,
        ParkingLotStatus status = ParkingLotStatus.Active)
    {
        return new ParkingLot
        {
            Id = DeterministicGuid(key),
            OwnerId = ownerId,
            Name = name,
            Address = address,
            Description = $"Bãi đỗ xe tại {address}.",
            CoverImageUrl = $"https://example.local/parking-lots/{key}.jpg",
            ContactPhone = "02839990000",
            Location = _geometryFactory.CreatePoint(new Coordinate(longitude, latitude)),
            TotalSlots = totalSlots,
            AvailableSlots = availableSlots,
            FirstBlockPrice = firstBlockPrice,
            FirstBlockHours = firstBlockHours,
            OvernightPrice = firstBlockPrice * 8,
            MaxHeight = maxHeight,
            HasRoof = hasRoof,
            Is24_7 = is24_7,
            OpenTime = openTime,
            CloseTime = closeTime,
            AverageRating = rating,
            Status = status
        };
    }

    private static List<ParkingFloor> CreateParkingFloors(IReadOnlyCollection<ParkingLot> parkingLots, IReadOnlyCollection<LayoutTemplate> templates)
    {
        var basementTemplate = templates.Single(t => t.Name == "Basement Grid 24");
        var outdoorTemplate = templates.Single(t => t.Name == "Outdoor Row 18");
        var angledTemplate = templates.Single(t => t.Name == "Angled Slots 30");

        return parkingLots.SelectMany(lot =>
        {
            var template = lot.HasRoof ? basementTemplate : outdoorTemplate;
            return lot.Name.Contains("Street Parking", StringComparison.OrdinalIgnoreCase)
                ? [CreateFloor(lot.Id, angledTemplate.Id, "Street Zone A"), CreateFloor(lot.Id, angledTemplate.Id, "Street Zone B")]
                : new[] { CreateFloor(lot.Id, template.Id, "B1") };
        }).ToList();
    }

    private static ParkingFloor CreateFloor(Guid parkingLotId, Guid templateId, string floorName)
    {
        return new ParkingFloor
        {
            Id = DeterministicGuid($"floor-{parkingLotId}-{floorName}"),
            ParkingLotId = parkingLotId,
            TemplateId = templateId,
            FloorName = floorName,
            CustomBackgroundImageUrl = null
        };
    }

    private static List<ParkingSlot> CreateParkingSlots(IReadOnlyCollection<ParkingFloor> floors)
    {
        var slots = new List<ParkingSlot>();

        foreach (var floor in floors)
        {
            var slotCount = floor.FloorName == "B1" ? 12 : 8;
            for (var index = 0; index < slotCount; index++)
            {
                var row = index / 6;
                var column = index % 6;
                var status = index switch
                {
                    0 or 5 => ParkingSlotStatus.Occupied,
                    10 => ParkingSlotStatus.Maintenance,
                    _ => ParkingSlotStatus.Available
                };

                slots.Add(new ParkingSlot
                {
                    Id = DeterministicGuid($"slot-{floor.Id}-{index + 1}"),
                    ParkingFloorId = floor.Id,
                    SlotName = $"{floor.FloorName}-{index + 1:00}",
                    Status = status,
                    VehicleType = SlotVehicleType.Car,
                    PositionX = 40 + column * 76,
                    PositionY = 40 + row * 120,
                    Width = 58,
                    Height = 96,
                    Rotation = floor.FloorName.StartsWith("Street", StringComparison.OrdinalIgnoreCase) ? -12 : 0
                });
            }
        }

        return slots;
    }

    private static List<Voucher> CreateVouchers()
    {
        return
        [
            new Voucher { Id = DeterministicGuid("voucher-free10k"), Code = "FREE10K", DiscountAmount = 10_000, ExpiryDate = DateTime.UtcNow.AddMonths(3), UsageLimit = 100 },
            new Voucher { Id = DeterministicGuid("voucher-welcome20"), Code = "WELCOME20", DiscountPercentage = 20, MaxDiscount = 25_000, ExpiryDate = DateTime.UtcNow.AddMonths(2), UsageLimit = 50 },
            new Voucher { Id = DeterministicGuid("voucher-office15"), Code = "OFFICE15", DiscountPercentage = 15, MaxDiscount = 20_000, ExpiryDate = DateTime.UtcNow.AddMonths(4), UsageLimit = 80 },
            new Voucher { Id = DeterministicGuid("voucher-night30"), Code = "NIGHT30", DiscountPercentage = 30, MaxDiscount = 35_000, ExpiryDate = DateTime.UtcNow.AddMonths(1), UsageLimit = 40 },
            new Voucher { Id = DeterministicGuid("voucher-flat20k"), Code = "FLAT20K", DiscountAmount = 20_000, ExpiryDate = DateTime.UtcNow.AddMonths(3), UsageLimit = 60 },
            new Voucher { Id = DeterministicGuid("voucher-vip50"), Code = "VIP50", DiscountPercentage = 50, MaxDiscount = 50_000, ExpiryDate = DateTime.UtcNow.AddMonths(1), UsageLimit = 20 }
        ];
    }

    private static List<ParkingLotStaff> CreateStaffAssignments(IReadOnlyCollection<User> users, IReadOnlyCollection<ParkingLot> parkingLots)
    {
        var guards = users
            .Where(u => u.Role == Role.Guard)
            .OrderBy(u => u.PhoneNumber)
            .ToList();

        var assignments = new List<ParkingLotStaff>();
        int guardIndex = 0;
        foreach (var lot in parkingLots)
        {
            for (int i = 0; i < 3; i++)
            {
                if (guardIndex < guards.Count)
                {
                    assignments.Add(new ParkingLotStaff
                    {
                        Id = DeterministicGuid($"staff-{guards[guardIndex].Id}-{lot.Id}"),
                        UserId = guards[guardIndex].Id,
                        ParkingLotId = lot.Id
                    });
                    guardIndex++;
                }
            }
        }
        return assignments;
    }

    private static List<FavouriteParkingLot> CreateFavouriteParkingLots(IReadOnlyCollection<User> users, IReadOnlyCollection<ParkingLot> parkingLots)
    {
        var drivers = users
            .Where(u => u.Role == Role.Driver)
            .OrderBy(u => u.PhoneNumber)
            .Take(9)
            .ToList();

        var lots = parkingLots
            .OrderBy(p => p.Name)
            .Take(9)
            .ToList();

        return drivers
            .Select((driver, index) => new FavouriteParkingLot
            {
                UserId = driver.Id,
                ParkingLotId = lots[index].Id
            })
            .ToList();
    }

    private static List<Notification> CreateNotifications(IReadOnlyCollection<User> users)
    {
        var now = DateTime.UtcNow;
        var notifications = new List<Notification>();

        foreach (var user in users)
        {
            var items = user.Role switch
            {
                Role.Driver => new[]
                {
                    ("booking-confirmed", "Đặt chỗ thành công", "Chỗ đỗ A01 đã được giữ. Vui lòng check-in đúng hạn.", false, 5),
                    ("checkin-reminder", "Sắp hết hạn check-in", "Bạn còn ít phút để check-in trước khi lượt đặt bị hủy.", false, 15),
                    ("payment-success", "Thanh toán thành công", "Thanh toán cho lượt đỗ gần nhất đã được xác nhận.", true, 180),
                    ("monthly-pass", "Vé tháng sắp hết hạn", "Vé tháng của bạn sẽ hết hạn trong 3 ngày.", true, 1440)
                },
                Role.Guard => new[]
                {
                    ("staff-assignment", "Phân công bãi xe", "Bạn đã được phân công trực tại một bãi đỗ.", false, 30)
                },
                Role.ParkingOwner => new[]
                {
                    ("owner-booking", "Có lượt đặt chỗ mới", "Bãi đỗ của bạn vừa nhận một lượt đặt chỗ mới.", false, 10),
                    ("owner-payment", "Đối soát thanh toán", "Báo cáo doanh thu mẫu đã sẵn sàng để kiểm tra.", true, 720)
                },
                Role.Admin => new[]
                {
                    ("system", "Thông báo hệ thống", "Dữ liệu mẫu đã được khởi tạo thành công.", false, 1)
                },
                _ => Array.Empty<(string, string, string, bool, int)>()
            };

            foreach (var (key, title, message, isRead, minutesAgo) in items)
            {
                notifications.Add(new Notification
                {
                    Id = DeterministicGuid($"notification-{key}-{user.Id}"),
                    UserId = user.Id,
                    Title = title,
                    Message = message,
                    IsRead = isRead,
                    CreatedAt = now.AddMinutes(-minutesAgo)
                });
            }
        }

        return notifications;
    }

    private static List<BankAccount> CreateBankAccounts(IReadOnlyCollection<User> users)
    {
        return users
            .Where(u => u.Role == Role.ParkingOwner)
            .Select(u => new BankAccount
            {
                Id = DeterministicGuid($"bank-{u.Id}"),
                UserId = u.Id,
                BankName = "Vietcombank",
                AccountNumber = $"1000{Math.Abs(u.Id.GetHashCode()) % 100000000:00000000}",
                AccountName = u.FullName.ToUpperInvariant(),
                IsDefault = true
            })
            .ToList();
    }

    private static List<MonthlyPass> CreateMonthlyPasses(IReadOnlyCollection<User> users, IReadOnlyCollection<Vehicle> vehicles, IReadOnlyCollection<ParkingLot> parkingLots)
    {
        var drivers = users
            .Where(u => u.Role == Role.Driver)
            .OrderBy(u => u.PhoneNumber)
            .Take(3)
            .ToList();

        var monthlyLots = parkingLots
            .Where(p => p.HasRoof || p.Is24_7)
            .OrderBy(p => p.Name)
            .Take(3)
            .ToList();

        return drivers
            .Select((driver, index) =>
            {
                var vehicle = vehicles.Single(v => v.UserId == driver.Id && v.IsDefault);
                var parkingLot = monthlyLots[index];

                return new MonthlyPass
                {
                    Id = DeterministicGuid($"monthly-pass-{driver.Id}-{parkingLot.Id}"),
                    UserId = driver.Id,
                    VehicleId = vehicle.Id,
                    ParkingLotId = parkingLot.Id,
                    StartDate = DateTime.UtcNow.Date.AddDays(-5 + index),
                    EndDate = DateTime.UtcNow.Date.AddDays(25 + index),
                    Price = 1_500_000 + index * 150_000,
                    Status = MonthlyPassStatus.Active
                };
            })
            .ToList();
    }

    private static List<Booking> CreateBookings(
        IReadOnlyCollection<User> users,
        IReadOnlyCollection<Vehicle> vehicles,
        IReadOnlyCollection<ParkingLot> parkingLots,
        IReadOnlyCollection<ParkingSlot> slots,
        IReadOnlyCollection<Voucher> vouchers)
    {
        var drivers = users
            .Where(u => u.Role == Role.Driver && !u.IsLocked)
            .OrderBy(u => u.PhoneNumber)
            .ToList();

        var lots = parkingLots
            .OrderBy(p => p.Name)
            .ToList();

        var free10k = vouchers.Single(v => v.Code == "FREE10K");
        var welcome20 = vouchers.Single(v => v.Code == "WELCOME20");
        var office15 = vouchers.Single(v => v.Code == "OFFICE15");
        var occupiedSlots = slots.Where(s => s.Status == ParkingSlotStatus.Occupied).ToList();
        var availableSlots = slots.Where(s => s.Status == ParkingSlotStatus.Available).ToList();

        var bookings = new List<Booking>
        {
            new Booking
            {
                Id = DeterministicGuid("booking-pending-driver1"),
                UserId = drivers[0].Id,
                VehicleId = vehicles.Single(v => v.UserId == drivers[0].Id && v.IsDefault).Id,
                ParkingLotId = lots[0].Id,
                ParkingSlotId = occupiedSlots[0].Id,
                VoucherId = free10k.Id,
                BookingCode = "A7B9K2",
                BookingTimestamp = DateTime.UtcNow.AddMinutes(-10),
                Status = BookingStatus.Pending
            },
            new Booking
            {
                Id = DeterministicGuid("booking-checkedin-driver2"),
                UserId = drivers[1].Id,
                VehicleId = vehicles.Single(v => v.UserId == drivers[1].Id && v.IsDefault).Id,
                ParkingLotId = lots[1].Id,
                ParkingSlotId = occupiedSlots[1].Id,
                BookingCode = "C3D8Q1",
                BookingTimestamp = DateTime.UtcNow.AddHours(-2),
                CheckInTimestamp = DateTime.UtcNow.AddHours(-1),
                Status = BookingStatus.CheckedIn
            },
            new Booking
            {
                Id = DeterministicGuid("booking-pending-driver4"),
                UserId = drivers[3].Id,
                VehicleId = vehicles.Single(v => v.UserId == drivers[3].Id && v.IsDefault).Id,
                ParkingLotId = lots[3].Id,
                ParkingSlotId = occupiedSlots[2].Id,
                VoucherId = welcome20.Id,
                BookingCode = "M4N5P6",
                BookingTimestamp = DateTime.UtcNow.AddMinutes(-18),
                Status = BookingStatus.Pending
            },
            new Booking
            {
                Id = DeterministicGuid("booking-checkedin-driver5"),
                UserId = drivers[4].Id,
                VehicleId = vehicles.Single(v => v.UserId == drivers[4].Id && v.IsDefault).Id,
                ParkingLotId = lots[4].Id,
                ParkingSlotId = occupiedSlots[3].Id,
                BookingCode = "R7S8T9",
                BookingTimestamp = DateTime.UtcNow.AddHours(-3),
                CheckInTimestamp = DateTime.UtcNow.AddHours(-2),
                Status = BookingStatus.CheckedIn
            },
            new Booking
            {
                Id = DeterministicGuid("booking-cancelled-driver7"),
                UserId = drivers[6].Id,
                VehicleId = vehicles.Single(v => v.UserId == drivers[6].Id && v.IsDefault).Id,
                ParkingLotId = lots[6].Id,
                ParkingSlotId = availableSlots[2].Id,
                VoucherId = office15.Id,
                BookingCode = "D4E5F6",
                BookingTimestamp = DateTime.UtcNow.AddDays(-1).AddHours(-2),
                Status = BookingStatus.Cancelled
            },
            new Booking
            {
                Id = DeterministicGuid("booking-noshow-driver8"),
                UserId = drivers[7].Id,
                VehicleId = vehicles.Single(v => v.UserId == drivers[7].Id && v.IsDefault).Id,
                ParkingLotId = lots[7].Id,
                ParkingSlotId = availableSlots[3].Id,
                BookingCode = "G7H8I9",
                BookingTimestamp = DateTime.UtcNow.AddDays(-1).AddHours(-5),
                Status = BookingStatus.NoShow
            }
        };

        // Generate 3 completed bookings for EVERY parking lot
        var random = new Random(42);
        foreach (var lot in parkingLots)
        {
            for (int i = 0; i < 3; i++)
            {
                var driver = drivers[random.Next(drivers.Count)];
                var vehicle = vehicles.First(v => v.UserId == driver.Id && v.IsDefault);
                var checkoutTime = DateTime.UtcNow.AddDays(-random.Next(1, 30)).AddHours(-random.Next(1, 10));
                
                bookings.Add(new Booking
                {
                    Id = DeterministicGuid($"booking-completed-{lot.Id}-{i}"),
                    UserId = driver.Id,
                    VehicleId = vehicle.Id,
                    ParkingLotId = lot.Id,
                    ParkingSlotId = availableSlots[random.Next(availableSlots.Count)].Id,
                    BookingCode = $"{(char)('A' + random.Next(26))}{(char)('A' + random.Next(26))}{random.Next(1000, 9999)}",
                    BookingTimestamp = checkoutTime.AddHours(-4),
                    CheckInTimestamp = checkoutTime.AddHours(-3),
                    CheckOutTimestamp = checkoutTime,
                    Status = BookingStatus.Completed,
                    TotalPrice = (decimal)(random.Next(20, 100) * 1000)
                });
            }
        }

        // Generate dozens of active/pending bookings
        for (int i = 0; i < 40; i++)
        {
            var driver = drivers[random.Next(drivers.Count)];
            var vehicle = vehicles.First(v => v.UserId == driver.Id && v.IsDefault);
            var lot = lots[random.Next(lots.Count)];
            var isPending = random.Next(2) == 0;
            var bookingTime = DateTime.UtcNow.AddMinutes(-random.Next(5, 120));

            bookings.Add(new Booking
            {
                Id = DeterministicGuid($"booking-active-{i}"),
                UserId = driver.Id,
                VehicleId = vehicle.Id,
                ParkingLotId = lot.Id,
                ParkingSlotId = availableSlots[random.Next(availableSlots.Count)].Id,
                BookingCode = $"{(char)('B' + random.Next(26))}{(char)('B' + random.Next(26))}{random.Next(1000, 9999)}",
                BookingTimestamp = bookingTime,
                CheckInTimestamp = isPending ? null : bookingTime.AddMinutes(15),
                Status = isPending ? BookingStatus.Pending : BookingStatus.CheckedIn
            });
        }

        // Generate violations for locked users
        var violators = users.Where(u => u.Role == Role.Driver && u.IsLocked).ToList();
        foreach (var violator in violators)
        {
            var vehicle = vehicles.First(v => v.UserId == violator.Id && v.IsDefault);
            for (int i = 0; i < 4; i++)
            {
                var lot = lots[random.Next(lots.Count)];
                bookings.Add(new Booking
                {
                    Id = DeterministicGuid($"booking-violation-{violator.Id}-{i}"),
                    UserId = violator.Id,
                    VehicleId = vehicle.Id,
                    ParkingLotId = lot.Id,
                    ParkingSlotId = availableSlots[random.Next(availableSlots.Count)].Id,
                    BookingCode = $"{(char)('V' + random.Next(26))}{(char)('V' + random.Next(26))}{random.Next(1000, 9999)}",
                    BookingTimestamp = DateTime.UtcNow.AddDays(-random.Next(1, 10)),
                    Status = random.Next(2) == 0 ? BookingStatus.NoShow : BookingStatus.Cancelled
                });
            }
        }
        
        return bookings;
    }

    private static List<Transaction> CreateTransactions(IReadOnlyCollection<Booking> bookings)
    {
        return bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .Select((booking, index) => new Transaction
            {
                Id = DeterministicGuid($"transaction-{booking.Id}"),
                BookingId = booking.Id,
                Amount = booking.TotalPrice ?? 0,
                PaymentMethod = index % 2 == 0 ? PaymentMethod.Wallet : PaymentMethod.VietQR,
                Status = TransactionStatus.Success,
                TransactionDate = booking.CheckOutTimestamp ?? DateTime.UtcNow
            })
            .ToList();
    }

    private static List<Review> CreateReviews(IReadOnlyCollection<User> users, IReadOnlyCollection<ParkingLot> parkingLots, IReadOnlyCollection<Booking> bookings)
    {
        return bookings
            .Where(b => b.Status == BookingStatus.Completed && b.UserId.HasValue)
            .Select((booking, index) => new Review
            {
                Id = DeterministicGuid($"review-{booking.Id}"),
                UserId = booking.UserId!.Value,
                ParkingLotId = booking.ParkingLotId,
                BookingId = booking.Id,
                Rating = 5 - index % 2,
                Comment = index % 2 == 0
                    ? "Clean, easy to find, fast check-out."
                    : "Good location, entry lane can be busy at peak time."
            })
            .ToList();
    }

    private static List<CrowdsourceReport> CreateCrowdsourceReports(IReadOnlyCollection<User> users, IReadOnlyCollection<ParkingLot> parkingLots)
    {
        var drivers = users
            .Where(u => u.Role == Role.Driver)
            .OrderBy(u => u.PhoneNumber)
            .Skip(2)
            .Take(3)
            .ToList();

        var lots = parkingLots
            .OrderBy(p => p.Name)
            .Take(3)
            .ToList();

        return drivers
            .Select((driver, index) => new CrowdsourceReport
            {
                Id = DeterministicGuid($"report-{driver.Id}-{lots[index].Id}"),
                UserId = driver.Id,
                ParkingLotId = lots[index].Id,
                ReportedStatus = index % 2 == 0 ? ReportStatus.Available : ReportStatus.Full,
                ReportedAt = DateTime.UtcNow.AddMinutes(-15 - index * 7),
                IsProcessed = false
            })
            .ToList();
    }

    /// <summary>
    /// Tạo dữ liệu phong phú, thực tế cho 4 tài khoản fast-login của màn hình demo:
    /// - driver-1  (0933000001): nhiều booking đủ trạng thái, lịch sử dày dặn
    /// - owner-1   (0911000001): bãi xe sở hữu có nhiều booking từ nhiều tài xế
    /// - guard-1   (0922000001): bãi trực có nhiều lượt đang chờ xử lý (Pending/CheckedIn)
    /// - admin     (0900000000): dữ liệu hệ thống nhìn vào thấy sôi động
    /// </summary>
    private static List<Booking> CreateDemoUserRichData(
        IReadOnlyCollection<User> users,
        IReadOnlyCollection<Vehicle> vehicles,
        IReadOnlyCollection<ParkingLot> parkingLots,
        IReadOnlyCollection<ParkingSlot> slots,
        IReadOnlyCollection<Voucher> vouchers)
    {
        var bookings = new List<Booking>();
        var rng = new Random(77);

        var allDrivers = users.Where(u => u.Role == Role.Driver && !u.IsLocked).OrderBy(u => u.PhoneNumber).ToList();
        var availableSlots = slots.Where(s => s.Status == ParkingSlotStatus.Available).ToList();
        var lots = parkingLots.OrderBy(p => p.Name).ToList();

        // ── Xác định các user demo ──────────────────────────────────────────
        var driver1 = users.Single(u => u.PhoneNumber == "0933000001");
        var owner1  = users.Single(u => u.PhoneNumber == "0911000001");
        var guard1  = users.Single(u => u.PhoneNumber == "0922000001");

        var driver1Vehicle = vehicles.First(v => v.UserId == driver1.Id && v.IsDefault);

        // Bãi xe của owner-1 (theo seed hiện tại: Vincom Đồng Khởi, Saigon Centre, Tao Đàn, Vincom Thủ Đức)
        var owner1Lots = parkingLots.Where(p => p.OwnerId == owner1.Id).ToList();

        // Bãi được phân công cho guard-1: guard-1 là guard đầu tiên → phân công bãi đầu tiên alphabetically
        var guard1Lot = lots.First();

        var free10k  = vouchers.Single(v => v.Code == "FREE10K");
        var welcome20 = vouchers.Single(v => v.Code == "WELCOME20");
        var vip50    = vouchers.Single(v => v.Code == "VIP50");

        string RandCode(string prefix = "") =>
            $"{prefix}{(char)('A' + rng.Next(26))}{(char)('A' + rng.Next(26))}{rng.Next(1000, 9999)}";

        ParkingSlot RandSlot() => availableSlots[rng.Next(availableSlots.Count)];
        ParkingLot  RandLot()  => lots[rng.Next(lots.Count)];

        // ════════════════════════════════════════════════════════════════════
        // 1. DRIVER-1 – Lịch sử đặt chỗ phong phú
        // ════════════════════════════════════════════════════════════════════



        // 1c. 3 booking đã hoàn thành – đủ bãi khác nhau, đủ thời gian khác nhau
        var driver1CompletedLots = lots.Take(10).ToList();
        for (int i = 0; i < 3; i++)
        {
            var lot = driver1CompletedLots[i % driver1CompletedLots.Count];
            var checkoutTime = DateTime.UtcNow.AddDays(-(i + 1)).AddHours(-rng.Next(1, 8));
            bookings.Add(new Booking
            {
                Id = DeterministicGuid($"demo-driver1-completed-{i}"),
                UserId = driver1.Id, VehicleId = driver1Vehicle.Id,
                ParkingLotId = lot.Id, ParkingSlotId = RandSlot().Id,
                VoucherId = i % 4 == 0 ? welcome20.Id : i % 7 == 0 ? vip50.Id : null,
                BookingCode = RandCode("D1"),
                BookingTimestamp  = checkoutTime.AddHours(-4),
                CheckInTimestamp  = checkoutTime.AddHours(-3),
                CheckOutTimestamp = checkoutTime,
                Status = BookingStatus.Completed,
                TotalPrice = (decimal)(rng.Next(20, 120) * 1000)
            });
        }

        // 1d. 1 booking bị Cancelled (để thấy lịch sử hủy)
        for (int i = 0; i < 1; i++)
        {
            bookings.Add(new Booking
            {
                Id = DeterministicGuid($"demo-driver1-cancelled-{i}"),
                UserId = driver1.Id, VehicleId = driver1Vehicle.Id,
                ParkingLotId = RandLot().Id, ParkingSlotId = RandSlot().Id,
                BookingCode = RandCode("CX"),
                BookingTimestamp = DateTime.UtcNow.AddDays(-(i + 2)).AddHours(-rng.Next(1, 6)),
                Status = BookingStatus.Cancelled
            });
        }

        // 1e. 1 booking NoShow
        for (int i = 0; i < 1; i++)
        {
            bookings.Add(new Booking
            {
                Id = DeterministicGuid($"demo-driver1-noshow-{i}"),
                UserId = driver1.Id, VehicleId = driver1Vehicle.Id,
                ParkingLotId = RandLot().Id, ParkingSlotId = RandSlot().Id,
                BookingCode = RandCode("NS"),
                BookingTimestamp = DateTime.UtcNow.AddDays(-(i + 5)).AddHours(-rng.Next(2, 10)),
                Status = BookingStatus.NoShow
            });
        }

        // ════════════════════════════════════════════════════════════════════
        // 2. CÁC BÃI CỦA OWNER-1 – Booking dày đặc từ nhiều tài xế
        // ════════════════════════════════════════════════════════════════════
        foreach (var ownerLot in owner1Lots)
        {
            // 1 booking Pending đang chờ check-in (guard cần xử lý)
            for (int i = 0; i < 1; i++)
            {
                var drv = allDrivers[rng.Next(allDrivers.Count)];
                var veh = vehicles.First(v => v.UserId == drv.Id && v.IsDefault);
                bookings.Add(new Booking
                {
                    Id = DeterministicGuid($"demo-owner1-lot-{ownerLot.Id}-pending-{i}"),
                    UserId = drv.Id, VehicleId = veh.Id,
                    ParkingLotId = ownerLot.Id, ParkingSlotId = RandSlot().Id,
                    BookingCode = RandCode("OW"),
                    BookingTimestamp = DateTime.UtcNow.AddMinutes(-rng.Next(3, 45)),
                    Status = BookingStatus.Pending
                });
            }

            // 1 booking CheckedIn (xe đang trong bãi)
            for (int i = 0; i < 1; i++)
            {
                var drv = allDrivers[rng.Next(allDrivers.Count)];
                var veh = vehicles.First(v => v.UserId == drv.Id && v.IsDefault);
                var bookingTime = DateTime.UtcNow.AddHours(-rng.Next(1, 4));
                bookings.Add(new Booking
                {
                    Id = DeterministicGuid($"demo-owner1-lot-{ownerLot.Id}-checkedin-{i}"),
                    UserId = drv.Id, VehicleId = veh.Id,
                    ParkingLotId = ownerLot.Id, ParkingSlotId = RandSlot().Id,
                    BookingCode = RandCode("CI"),
                    BookingTimestamp = bookingTime,
                    CheckInTimestamp  = bookingTime.AddMinutes(rng.Next(5, 30)),
                    Status = BookingStatus.CheckedIn
                });
            }

            // 2 booking Completed gần đây (revenue report)
            for (int i = 0; i < 2; i++)
            {
                var drv = allDrivers[rng.Next(allDrivers.Count)];
                var veh = vehicles.First(v => v.UserId == drv.Id && v.IsDefault);
                var checkoutTime = DateTime.UtcNow.AddDays(-rng.Next(1, 14)).AddHours(-rng.Next(1, 8));
                bookings.Add(new Booking
                {
                    Id = DeterministicGuid($"demo-owner1-lot-{ownerLot.Id}-completed-{i}"),
                    UserId = drv.Id, VehicleId = veh.Id,
                    ParkingLotId = ownerLot.Id, ParkingSlotId = RandSlot().Id,
                    BookingCode = RandCode("OC"),
                    BookingTimestamp  = checkoutTime.AddHours(-5),
                    CheckInTimestamp  = checkoutTime.AddHours(-4),
                    CheckOutTimestamp = checkoutTime,
                    Status = BookingStatus.Completed,
                    TotalPrice = (decimal)(rng.Next(15, 200) * 1000)
                });
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // 3. BÃI CỦA GUARD-1 – Nhiều lượt đang chờ guard xử lý
        // ════════════════════════════════════════════════════════════════════
        // guard-1 trực bãi đầu tiên, tạo 2 booking Pending + 1 CheckedIn tại đó
        for (int i = 0; i < 2; i++)
        {
            var drv = allDrivers[rng.Next(allDrivers.Count)];
            var veh = vehicles.First(v => v.UserId == drv.Id && v.IsDefault);
            bookings.Add(new Booking
            {
                Id = DeterministicGuid($"demo-guard1-lot-pending-{i}"),
                UserId = drv.Id, VehicleId = veh.Id,
                ParkingLotId = guard1Lot.Id, ParkingSlotId = RandSlot().Id,
                VoucherId = i % 5 == 0 ? free10k.Id : null,
                BookingCode = RandCode("GP"),
                BookingTimestamp = DateTime.UtcNow.AddMinutes(-rng.Next(2, 60)),
                Status = BookingStatus.Pending
            });
        }

        for (int i = 0; i < 1; i++)
        {
            var drv = allDrivers[rng.Next(allDrivers.Count)];
            var veh = vehicles.First(v => v.UserId == drv.Id && v.IsDefault);
            var btime = DateTime.UtcNow.AddHours(-rng.Next(1, 5));
            bookings.Add(new Booking
            {
                Id = DeterministicGuid($"demo-guard1-lot-checkedin-{i}"),
                UserId = drv.Id, VehicleId = veh.Id,
                ParkingLotId = guard1Lot.Id, ParkingSlotId = RandSlot().Id,
                BookingCode = RandCode("GC"),
                BookingTimestamp = btime,
                CheckInTimestamp  = btime.AddMinutes(rng.Next(5, 25)),
                Status = BookingStatus.CheckedIn
            });
        }

        return bookings;
    }

    private static Guid DeterministicGuid(string value)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        return new Guid(bytes);
    }
}
