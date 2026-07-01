using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using parking_booking_backend.DTOs;
using parking_booking_backend.Interfaces;

namespace parking_booking_backend.Controllers;

[Authorize]
[ApiController]
[Route("api/wallets")]
public sealed class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<WalletResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var wallet = await _walletService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<WalletResponse>.Ok(wallet));
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<ApiResponse<DepositResponse>>> Deposit(DepositRequest request, CancellationToken cancellationToken)
    {
        var deposit = await _walletService.CreateDepositAsync(request, cancellationToken);
        return Ok(ApiResponse<DepositResponse>.Ok(deposit));
    }

    [HttpGet("admin/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminWalletStatsResponse>>> GetAdminStats(CancellationToken cancellationToken)
    {
        var stats = await _walletService.GetAdminStatsAsync(cancellationToken);
        return Ok(ApiResponse<AdminWalletStatsResponse>.Ok(stats));
    }

    [HttpGet("admin/users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PaginationResponse<AdminUserWalletResponse>>>> GetAdminUserWallets(
        [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? keyword = null, CancellationToken cancellationToken = default)
    {
        var wallets = await _walletService.GetAdminUserWalletsAsync(page, size, keyword, cancellationToken);
        return Ok(ApiResponse<PaginationResponse<AdminUserWalletResponse>>.Ok(wallets));
    }
}
