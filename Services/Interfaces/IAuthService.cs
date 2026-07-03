using UserApi.Common;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;

namespace UserApi.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<UserResponse>> GetCurrentUserAsync(int userId);
}