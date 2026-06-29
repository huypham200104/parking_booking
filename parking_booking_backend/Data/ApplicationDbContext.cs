using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using parking_booking_backend.Models;

namespace parking_booking_backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ParkingLot> ParkingLots { get; set; }
        public DbSet<LayoutTemplate> LayoutTemplates { get; set; }
        public DbSet<ParkingFloor> ParkingFloors { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CrowdsourceReport> CrowdsourceReports { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ParkingLotStaff> ParkingLotStaffs { get; set; }
        public DbSet<MonthlyPass> MonthlyPasses { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<FavouriteParkingLot> FavouriteParkingLots { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureSoftDeleteFilters(modelBuilder);
            ConfigureValueConstraints(modelBuilder);

            // Configure Users
            modelBuilder.Entity<User>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => new { v.UserId, v.LicensePlate })
                .IsUnique();

            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.UserId)
                .IsUnique();

            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Code)
                .IsUnique();

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.BookingCode)
                .IsUnique();

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.BookingId)
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.BookingId)
                .IsUnique();

            modelBuilder.Entity<ParkingLotStaff>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            modelBuilder.Entity<ParkingLot>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedParkingLots)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure FavouriteParkingLots Composite Key
            modelBuilder.Entity<FavouriteParkingLot>()
                .HasKey(f => new { f.UserId, f.ParkingLotId });

            modelBuilder.Entity<FavouriteParkingLot>()
                .HasQueryFilter(f => f.User != null && !f.User.IsDeleted && f.ParkingLot != null && !f.ParkingLot.IsDeleted);

            modelBuilder.Entity<FavouriteParkingLot>()
                .HasOne(f => f.User)
                .WithMany(u => u.FavouriteParkingLots)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FavouriteParkingLot>()
                .HasOne(f => f.ParkingLot)
                .WithMany(p => p.FavouriteByUsers)
                .HasForeignKey(f => f.ParkingLotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Wallet 1-1 Relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Wallet)
                .WithOne(w => w.User)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Booking -> Review 1-1 Relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Review)
                .WithOne(r => r.Booking)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Booking -> Transaction 1-1 Relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Transaction)
                .WithOne(t => t.Booking)
                .HasForeignKey<Transaction>(t => t.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent cascade loops on Staff
            modelBuilder.Entity<ParkingLotStaff>()
                .HasOne(ps => ps.User)
                .WithMany(u => u.ParkingLotStaffs)
                .HasForeignKey(ps => ps.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParkingLotStaff>()
                .HasOne(ps => ps.ParkingLot)
                .WithMany(p => p.ParkingLotStaffs)
                .HasForeignKey(ps => ps.ParkingLotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Allow nullable User and Vehicle on Bookings
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ParkingLot)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.ParkingLotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.User)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParkingFloor>()
                .HasOne(f => f.ParkingLot)
                .WithMany(p => p.ParkingFloors)
                .HasForeignKey(f => f.ParkingLotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ParkingFloor>()
                .HasOne(f => f.Template)
                .WithMany(t => t.ParkingFloors)
                .HasForeignKey(f => f.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ParkingSlot>()
                .HasOne(s => s.ParkingFloor)
                .WithMany(f => f.ParkingSlots)
                .HasForeignKey(s => s.ParkingFloorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CrowdsourceReport>()
                .HasOne(r => r.User)
                .WithMany(u => u.CrowdsourceReports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CrowdsourceReport>()
                .HasOne(r => r.ParkingLot)
                .WithMany(p => p.CrowdsourceReports)
                .HasForeignKey(r => r.ParkingLotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.ParkingLot)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ParkingLotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MonthlyPass>()
                .HasOne(p => p.User)
                .WithMany(u => u.MonthlyPasses)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MonthlyPass>()
                .HasOne(p => p.Vehicle)
                .WithMany(v => v.MonthlyPasses)
                .HasForeignKey(p => p.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MonthlyPass>()
                .HasOne(p => p.ParkingLot)
                .WithMany(l => l.MonthlyPasses)
                .HasForeignKey(p => p.ParkingLotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BankAccount>()
                .HasOne(a => a.User)
                .WithMany(u => u.BankAccounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(t => t.Wallet)
                .WithMany(w => w.WalletTransactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Vehicle>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ParkingLot>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<LayoutTemplate>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ParkingFloor>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ParkingSlot>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Booking>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Transaction>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<CrowdsourceReport>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ParkingLotStaff>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<MonthlyPass>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Wallet>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<WalletTransaction>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Voucher>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<BankAccount>().HasQueryFilter(e => !e.IsDeleted);
        }

        private void ConfigureValueConstraints(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.FullName).HasMaxLength(150);
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.Property(e => e.LicensePlate).HasMaxLength(20);
            });

            modelBuilder.Entity<ParkingLot>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.CoverImageUrl).HasMaxLength(1000);
                entity.Property(e => e.ContactPhone).HasMaxLength(20);
                if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
                {
                    entity.Property(e => e.Location).HasColumnType("geography");
                }
                entity.Property(e => e.FirstBlockPrice).HasPrecision(18, 2);
                entity.Property(e => e.OvernightPrice).HasPrecision(18, 2);
                entity.Property(e => e.MaxHeight).HasPrecision(5, 2);
            });

            modelBuilder.Entity<ParkingFloor>(entity =>
            {
                entity.Property(e => e.FloorName).HasMaxLength(100);
                entity.Property(e => e.CustomBackgroundImageUrl).HasMaxLength(1000);
            });

            modelBuilder.Entity<ParkingSlot>(entity =>
            {
                entity.Property(e => e.SlotName).HasMaxLength(50);
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(e => e.GuestLicensePlate).HasMaxLength(20);
                entity.Property(e => e.BookingCode).HasMaxLength(12);
                entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.Property(e => e.Comment).HasMaxLength(1000);
            });

            modelBuilder.Entity<MonthlyPass>(entity =>
            {
                entity.Property(e => e.Price).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.Property(e => e.Balance).HasPrecision(18, 2);
            });

            modelBuilder.Entity<WalletTransaction>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.ReferenceId).HasMaxLength(100);
            });

            modelBuilder.Entity<Voucher>(entity =>
            {
                entity.Property(e => e.Code).HasMaxLength(50);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.MaxDiscount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Message).HasMaxLength(1000);
            });

            modelBuilder.Entity<BankAccount>(entity =>
            {
                entity.Property(e => e.BankName).HasMaxLength(100);
                entity.Property(e => e.AccountNumber).HasMaxLength(50);
                entity.Property(e => e.AccountName).HasMaxLength(150);
            });

            modelBuilder.Entity<LayoutTemplate>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.ImageUrl).HasMaxLength(1000);
            });
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
