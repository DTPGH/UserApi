namespace UserApi.DTOs.Responses;

public class DashboardSummaryResponse
{
    public int TotalProjects { get; set; }

    public int TotalTasks { get; set; }

    public int TodoTasks { get; set; }

    public int InProgressTasks { get; set; }

    public int DoneTasks { get; set; }

    public int TotalUsers { get; set; }

    public int DeletedUsers { get; set; }
}