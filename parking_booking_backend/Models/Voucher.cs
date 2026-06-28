using System;
using System.Collections.Generic;

namespace parking_booking_backend.Models
{
    public class Voucher : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public decimal? DiscountAmount { get; set; }
        public float? DiscountPercentage { get; set; }
        public decimal? MaxDiscount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int UsageLimit { get; set; }

        // Navigation Properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
