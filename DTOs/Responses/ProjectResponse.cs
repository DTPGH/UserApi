namespace UserApi.DTOs.Responses;

public class ProjectResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
}