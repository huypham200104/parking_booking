using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/layout-templates")]
public sealed class LayoutTemplatesController : ControllerBase
{
    private readonly ILayoutService _layoutService;

    public LayoutTemplatesController(ILayoutService layoutService)
    {
        _layoutService = layoutService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LayoutTemplateResponse>>>> GetTemplates(CancellationToken cancellationToken)
    {
        var templates = await _layoutService.GetTemplatesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<LayoutTemplateResponse>>.Ok(templates));
    }
}

