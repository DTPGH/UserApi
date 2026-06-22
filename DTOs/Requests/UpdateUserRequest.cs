using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs.Requests;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Tên người dùng không được để trống")]
    [StringLength(255, ErrorMessage = "Tên không vượt quá 255 ký tự")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
    public string Email { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Des không vượt quá 1000 ký tự")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "Tuổi nằm trong khoảng 1 đến 120")]
    public int Age { get; set; }
}