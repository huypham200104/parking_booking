using System;

namespace parking_booking_backend.Models
{
    public class ParkingLotStaff : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid ParkingLotId { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual ParkingLot? ParkingLot { get; set; }
    }
}
