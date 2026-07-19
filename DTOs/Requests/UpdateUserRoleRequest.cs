using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs.Requests;

public class UpdateUserRoleRequest
{
    [Required]
    public string Role { get; set; } = "";
}