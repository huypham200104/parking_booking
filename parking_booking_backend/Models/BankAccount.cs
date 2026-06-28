using System;

namespace parking_booking_backend.Models
{
    public class BankAccount : BaseEntity
    {
        public Guid UserId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
    }
}
