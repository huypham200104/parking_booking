using System;
using System.Collections.Generic;

namespace parking_booking_backend.Models
{
    public class Wallet : BaseEntity
    {
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    }
}
