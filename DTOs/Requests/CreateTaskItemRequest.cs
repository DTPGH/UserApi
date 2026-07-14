using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs.Requests;

public class CreateTaskItemRequest
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }
}