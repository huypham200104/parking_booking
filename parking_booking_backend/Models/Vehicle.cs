using System;
using System.Collections.Generic;

namespace parking_booking_backend.Models
{
    public class Vehicle : BaseEntity
    {
        public Guid UserId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public VehicleType VehicleType { get; set; }
        public bool IsDefault { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<MonthlyPass> MonthlyPasses { get; set; } = new List<MonthlyPass>();
    }
}
