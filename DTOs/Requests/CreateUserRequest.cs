using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs.Requests;

public class CreateUserRequest
{
    [Required(ErrorMessage = "Tên người dùng không được để trống")]
    [StringLength(255, ErrorMessage = "Không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(255, ErrorMessage = "Không được vượt quá 255 ký tự ")]
    public string Email { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "Tuổi phải nằm trong khoảng 1 - 120  ")]
    public int Age { get; set; }

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = "";

    [Required]
    public string Role { get; set; } = "User";
}