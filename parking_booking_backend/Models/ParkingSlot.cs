using System;
using System.Collections.Generic;

namespace parking_booking_backend.Models
{
    public class ParkingSlot : BaseEntity
    {
        public Guid ParkingFloorId { get; set; }
        public string SlotName { get; set; } = string.Empty;
        public ParkingSlotStatus Status { get; set; }
        public SlotVehicleType VehicleType { get; set; }

        // UI Drawing properties
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Rotation { get; set; }

        // Navigation Properties
        public virtual ParkingFloor? ParkingFloor { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
