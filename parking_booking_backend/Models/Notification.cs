using System;

namespace parking_booking_backend.Models
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
    }
}
