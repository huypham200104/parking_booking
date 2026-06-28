using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace parking_booking_backend.Models
{
    public class ParkingLot : BaseEntity
    {
        public Guid OwnerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;

        public Point Location { get; set; } = null!;

        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }

        // Pricing
        public decimal FirstBlockPrice { get; set; }
        public int FirstBlockHours { get; set; }
        public decimal? OvernightPrice { get; set; }

        // Features & Hours
        public decimal? MaxHeight { get; set; }
        public bool HasRoof { get; set; }
        public bool Is24_7 { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }

        public float AverageRating { get; set; }
        public ParkingLotStatus Status { get; set; }

        // Navigation Properties
        public virtual User? Owner { get; set; }
        public virtual ICollection<ParkingFloor> ParkingFloors { get; set; } = new List<ParkingFloor>();
        public virtual ICollection<ParkingLotStaff> ParkingLotStaffs { get; set; } = new List<ParkingLotStaff>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CrowdsourceReport> CrowdsourceReports { get; set; } = new List<CrowdsourceReport>();
        public virtual ICollection<MonthlyPass> MonthlyPasses { get; set; } = new List<MonthlyPass>();
        public virtual ICollection<FavouriteParkingLot> FavouriteByUsers { get; set; } = new List<FavouriteParkingLot>();
    }
}
