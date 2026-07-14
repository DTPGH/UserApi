using UserApi.Models;

namespace UserApi.DTOs.Responses;

public class TaskItemResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = "";
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
}