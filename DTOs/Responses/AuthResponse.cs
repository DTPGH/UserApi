namespace UserApi.DTOs.Responses;

public class AuthResponse
{
    public string AccessToken { get; set; } = "";
    public DateTime AccessTokenExpiresAt { get; set; }
    public string RefreshToken { get; set; } = "";
    public DateTime RefreshTokenExpiresAt { get; set; }
    public UserResponse User { get; set; } = new();
}