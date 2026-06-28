using System;

namespace parking_booking_backend.Models
{
    public class CrowdsourceReport : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid ParkingLotId { get; set; }

        public ReportStatus ReportedStatus { get; set; }
        public DateTime ReportedAt { get; set; }
        public bool IsProcessed { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual ParkingLot? ParkingLot { get; set; }
    }
}
