using System;

namespace parking_booking_backend.Models
{
    public class Booking : BaseEntity
    {
        public Guid? UserId { get; set; }
        public Guid? VehicleId { get; set; }
        public string? GuestLicensePlate { get; set; }
        public Guid ParkingLotId { get; set; }
        public Guid ParkingSlotId { get; set; }
        public Guid? VoucherId { get; set; }

        public string BookingCode { get; set; } = string.Empty;
        public DateTime BookingTimestamp { get; set; }
        public DateTime? CheckInTimestamp { get; set; }
        public DateTime? CheckOutTimestamp { get; set; }

        public BookingStatus Status { get; set; }
        public decimal? TotalPrice { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
        public virtual ParkingLot? ParkingLot { get; set; }
        public virtual ParkingSlot? ParkingSlot { get; set; }
        public virtual Voucher? Voucher { get; set; }
        public virtual Transaction? Transaction { get; set; }
        public virtual Review? Review { get; set; }
    }
}
