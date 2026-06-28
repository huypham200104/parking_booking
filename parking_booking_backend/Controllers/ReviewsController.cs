using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> Create(CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await _reviewService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Create), new { id = review.Id }, ApiResponse<ReviewResponse>.Ok(review));
    }
}
