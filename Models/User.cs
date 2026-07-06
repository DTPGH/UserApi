using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore;
using UserApi.Models.Common;

namespace UserApi.Models;

[Table("Users")]
[Index(nameof(Email), IsUnique = true)]
public class User : BaseEntity
{
    // Primary key auto-increment
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    // tên người dùng, bắt buộc
    [Required]
    [StringLength(255)]
    [Column(TypeName = "nvarchar(255)")]
    public string Name { get; set; } = string.Empty;

    // email người dùng, bắt buộc, duy nhất
    [Required]
    [EmailAddress]
    [StringLength(255)]
    [Column(TypeName = "nvarchar(255)")]
    public string Email { get; set; } = string.Empty;

    // mô tả, cho phép null
    [Column(TypeName = "nvarchar(1000)")]
    public string? Description { get; set; }

    // trường tuổi có giá trị số phù hợp với tuổi người dùng
    [Range(0, 120)]
    public int Age { get; set; }

    // thêm field phục vụ cho đăng nhập
    [Required]
    [Column(TypeName = "nvarchar(255)")]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "nvarchar(255)")]
    public string Role { get; set; } = "User"; // Default role is "User"

    // Tạm thời lưu trường refreshtoken vào bảng user
    [Column(TypeName = "nvarchar(255)")]
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();

}