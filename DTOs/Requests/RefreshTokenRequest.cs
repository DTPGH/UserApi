using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs.Requests;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}