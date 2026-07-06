using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs.Requests;

public class UpdateProjectRequest
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }
}