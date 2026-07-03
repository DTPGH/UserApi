using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs.Requests;

public class RegisterRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = "";

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = "";

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "";

    [Range(0, 120)]
    public int Age { get; set; }

    public string? Description { get; set; }

}