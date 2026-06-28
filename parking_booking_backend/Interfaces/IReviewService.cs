using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using parking_booking_backend.DTOs;

namespace parking_booking_backend.Interfaces;

public interface IReviewService
{
    Task<ReviewResponse> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ReviewResponse>> GetByParkingLotAsync(Guid parkingLotId, CancellationToken cancellationToken);
}
