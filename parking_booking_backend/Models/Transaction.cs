using System;

namespace parking_booking_backend.Models
{
    public class Transaction : BaseEntity
    {
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public TransactionStatus Status { get; set; }
        public DateTime TransactionDate { get; set; }

        // Navigation Properties
        public virtual Booking? Booking { get; set; }
    }
}
