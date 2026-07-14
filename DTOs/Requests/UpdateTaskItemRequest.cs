using System.ComponentModel.DataAnnotations;
using UserApi.Models;

namespace UserApi.DTOs.Requests;

public class UpdateTaskItemRequest
{
    [Required]
    [StringLength(255)]
    public string Title { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }

    public DateTime? DueDate { get; set; }
}