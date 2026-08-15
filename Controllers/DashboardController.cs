using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApi.DTOs.Responses;
using UserApi.Extensions;
using UserApi.Services.Interfaces;

namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]

public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<DashboardSummaryResponse>>> GetSummary()
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<DashboardSummaryResponse>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _dashboardService.GetSummaryAsync(currentUserId.Value, currentUserRole);
        return this.ToActionResult(result);
    }
}