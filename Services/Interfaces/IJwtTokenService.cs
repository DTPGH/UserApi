using UserApi.Models;

namespace UserApi.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, out DateTime expiresAt);
    string GenerateRefreshToken();
}