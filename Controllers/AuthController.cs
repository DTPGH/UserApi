using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Extensions;
using UserApi.Services.Interfaces;
using System.Security.Claims;

namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return this.ToActionResult(result, StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request
    )
    {
        var result = await _authService.LoginAsync(request);
        return this.ToActionResult(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Me()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(userIdClaim, out var userId) == false)
        {
            return Unauthorized(new ApiResponse<UserResponse>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _authService.GetCurrentUserAsync(userId);

        return this.ToActionResult(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return this.ToActionResult(result);
    }
}