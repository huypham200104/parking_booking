using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("webhook")]
    public async Task<ActionResult<ApiResponse<object>>> Webhook(PaymentWebhookRequest request, CancellationToken cancellationToken)
    {
        var matched = await _paymentService.ProcessWebhookAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { matched }));
    }
}
