using System;

namespace parking_booking_backend.Models
{
    public class MonthlyPass : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid ParkingLotId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public MonthlyPassStatus Status { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual ParkingLot? ParkingLot { get; set; }
    }
}
