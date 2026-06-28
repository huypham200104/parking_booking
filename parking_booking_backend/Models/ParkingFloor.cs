using System;
using System.Collections.Generic;

namespace parking_booking_backend.Models
{
    public class ParkingFloor : BaseEntity
    {
        public Guid ParkingLotId { get; set; }
        public string FloorName { get; set; } = string.Empty;
        public Guid? TemplateId { get; set; }
        public string? CustomBackgroundImageUrl { get; set; }

        // Navigation Properties
        public virtual ParkingLot? ParkingLot { get; set; }
        public virtual LayoutTemplate? Template { get; set; }
        public virtual ICollection<ParkingSlot> ParkingSlots { get; set; } = new List<ParkingSlot>();
    }
}
