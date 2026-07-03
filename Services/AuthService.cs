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

    public AuthService(
        AppDbContext context,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger
    )
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
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

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtTokenService.GenerateAccessToken(user, out var expiresAt);

        _logger.LogInformation("Registerd new user {UserId}", user.Id);

        return ServiceResult<AuthResponse>.Ok(
            new AuthResponse
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
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

        var token = _jwtTokenService.GenerateAccessToken(user, out var expiresAt);

        _logger.LogInformation("User {UserId} logged in ", user.Id);

        return ServiceResult<AuthResponse>.Ok(
            new AuthResponse
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
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

}