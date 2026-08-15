using Microsoft.EntityFrameworkCore;
using UserApi.Common;
using UserApi.Data;
using UserApi.DTOs.Responses;
using UserApi.Models;
using UserApi.Services.Interfaces;

namespace UserApi.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<DashboardSummaryResponse>> GetSummaryAsync(int currentUserId, string currentUserRole)
    {
        // nếu currentUserRole là "Admin" thì isAdmin = true
        var isAdmin = currentUserRole == "Admin";

        var projectsQuery = _context.Projects.AsNoTracking();

        if (isAdmin == false)
        {
            projectsQuery = projectsQuery.Where(p => p.OwnerId == currentUserId);
        }

        var projectIds = await projectsQuery.Select(p => p.Id).ToListAsync();

        var tasksQuery = _context.TaskItems.AsNoTracking().Where(t => projectIds.Contains(t.ProjectId));

        var summary = new DashboardSummaryResponse
        {
            TotalProjects = projectIds.Count,
            TotalTasks = await tasksQuery.CountAsync(),
            TodoTasks = await tasksQuery.CountAsync(t => t.Status == TaskItemStatus.Todo),
            InProgressTasks = await tasksQuery.CountAsync(t => t.Status == TaskItemStatus.InProgress),
            DoneTasks = await tasksQuery.CountAsync(t => t.Status == TaskItemStatus.Done),
        };

        if (isAdmin)
        {
            summary.TotalUsers = await _context.Users.CountAsync(u => u.Deleted == false);

            summary.DeletedUsers = await _context.Users.CountAsync(u => u.Deleted == true);
        }

        return ServiceResult<DashboardSummaryResponse>.Ok(summary, "Lấy thông kê dashboard thành công.");
    }
}