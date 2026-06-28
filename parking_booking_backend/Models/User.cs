using System;
using System.Collections.Generic;

namespace parking_booking_backend.Models
{
    public class User : BaseEntity
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public Role Role { get; set; }
        public int TrustScore { get; set; } = 100;

        // Navigation Properties
        public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public virtual ICollection<ParkingLot> OwnedParkingLots { get; set; } = new List<ParkingLot>();
        public virtual ICollection<ParkingLotStaff> ParkingLotStaffs { get; set; } = new List<ParkingLotStaff>();
        public virtual Wallet? Wallet { get; set; }
        public virtual ICollection<MonthlyPass> MonthlyPasses { get; set; } = new List<MonthlyPass>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<FavouriteParkingLot> FavouriteParkingLots { get; set; } = new List<FavouriteParkingLot>();
        public virtual ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CrowdsourceReport> CrowdsourceReports { get; set; } = new List<CrowdsourceReport>();
    }
}
