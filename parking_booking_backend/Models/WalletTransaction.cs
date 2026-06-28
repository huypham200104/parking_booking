using System;

namespace parking_booking_backend.Models
{
    public class WalletTransaction : BaseEntity
    {
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public WalletTransactionType Type { get; set; }
        public string ReferenceId { get; set; } = string.Empty; // FK to BookingId or Payment Gateway transaction ID

        // Navigation Properties
        public virtual Wallet? Wallet { get; set; }
    }
}
