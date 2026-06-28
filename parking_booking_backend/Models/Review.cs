using System;

namespace parking_booking_backend.Models
{
    public class Review : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid ParkingLotId { get; set; }
        public Guid BookingId { get; set; }

        public int Rating { get; set; }
        public string? Comment { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual ParkingLot? ParkingLot { get; set; }
        public virtual Booking? Booking { get; set; }
    }
}
