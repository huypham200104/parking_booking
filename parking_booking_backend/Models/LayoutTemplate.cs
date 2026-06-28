using System;
using System.Collections.Generic;

namespace parking_booking_backend.Models
{
    public class LayoutTemplate : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation Properties
        public virtual ICollection<ParkingFloor> ParkingFloors { get; set; } = new List<ParkingFloor>();
    }
}
