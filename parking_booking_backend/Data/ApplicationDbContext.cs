using Microsoft.EntityFrameworkCore;

namespace parking_booking_backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tương lai bạn sẽ khai báo các DbSet ở đây. Ví dụ:
        // public DbSet<User> Users { get; set; }
    }
}
