namespace UserApi.DTOs.Responses;

public class AuthResponse
{
    public string AccessToken { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public UserResponse User { get; set; } = new();
}