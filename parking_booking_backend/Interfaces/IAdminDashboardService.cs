using parking_booking_backend.DTOs;

namespace parking_booking_backend.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardResponse> GetAsync(CancellationToken cancellationToken);
}
