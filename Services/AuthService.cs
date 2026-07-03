using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserApi.Common;
using UserApi.Data;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Models;
using UserApi.Services.Interfaces;

namespace UserApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;

    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IConfiguration configuration
    )
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _configuration = configuration;
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Description = user.Description,
            Age = user.Age
        };
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
        {
            return ServiceResult<AuthResponse>.Fail(
                "Email đã tồn tại",
                ServiceErrorType.Conflict
            );
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Description = request.Description,
            Age = request.Age,
            Role = "User"
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            int.Parse(_configuration["Jwt:RefreshTokenExpiresInDays"] ?? "7")
        );

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, out var accessTokenExpiresAt);

        _logger.LogInformation("Registerd new user {UserId}", user.Id);

        return ServiceResult<AuthResponse>.Ok(
            new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                User = MapToUserResponse(user)
            },

            "Đăng ký thành công"
        );
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Deleted == false);

        if (user is null)
        {
            return ServiceResult<AuthResponse>.Fail(
                "Email hoặc mật khẩu không chính xác",
                ServiceErrorType.BadRequest
            );
        }

        var passwordHasher = new PasswordHasher<User>();
        var verifyResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return ServiceResult<AuthResponse>.Fail(
                "Email hoặc mật khẩu không chính xác",
                ServiceErrorType.BadRequest
            );
        }

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            int.Parse(_configuration["Jwt:RefreshTokenExpiresInDays"] ?? "7")
        );

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;

        await _context.SaveChangesAsync();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, out var accessTokenExpiresAt);

        _logger.LogInformation("User {UserId} logged in ", user.Id);

        return ServiceResult<AuthResponse>.Ok(
            new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                User = MapToUserResponse(user)
            },
            "Đăng nhập thành công "
        );

    }

    public async Task<ServiceResult<UserResponse>> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.Deleted == false);

        if (user == null)
        {
            return ServiceResult<UserResponse>.Fail(
                "Không tìm thấy người dùng hiện tại",
                ServiceErrorType.NotFound
            );
        }

        return ServiceResult<UserResponse>.Ok(
            MapToUserResponse(user),
            "Lấy thông tin người dùng hiện tại thành công"
        );
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && u.Deleted == false);

        if (user == null)
        {
            return ServiceResult<AuthResponse>.Fail(
                "Refresh token không hợp lệ",
                ServiceErrorType.BadRequest
            );
        }

        if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
        {
            return ServiceResult<AuthResponse>.Fail(
                "Refresh token đã hết hạn",
                ServiceErrorType.BadRequest
            );
        }

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            int.Parse(_configuration["Jwt:RefreshTokenExpiresInDays"] ?? "7")
        );

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = newRefreshTokenExpiresAt;

        await _context.SaveChangesAsync();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, out var accessTokenExpiresAt);

        return ServiceResult<AuthResponse>.Ok(
            new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = newRefreshTokenExpiresAt,
                User = MapToUserResponse(user)
            },
            "Làm mới token thành công"
        );

    }
}