using Microsoft.EntityFrameworkCore;
using UserApi.Common;
using UserApi.Data;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Models;
using UserApi.Services.Interfaces;

namespace UserApi.Services;

public class TaskItemService : ITaskItemService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TaskItemService> _logger;
    public TaskItemService(AppDbContext context, ILogger<TaskItemService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public static bool CanAccessProject(Project project, int currentUserId, string currentUserRole)
    {
        return currentUserRole == "Admin" || project.OwnerId == currentUserId;
    }

    public static TaskItemResponse MapToTaskItemResponse(TaskItem task)
    {
        return new TaskItemResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId,
            ProjectName = task.Project.Name,
            OwnerId = task.Project.OwnerId,
            OwnerName = task.Project.Owner.Name
        };
    }

    public async Task<ServiceResult<List<TaskItemResponse>>> GetTasksByProjectAsync(int projectId, int currentUserId, string currentUserRole)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return ServiceResult<List<TaskItemResponse>>.Fail(
                "Không tìm thấy project",
                ServiceErrorType.NotFound);
        }

        if (CanAccessProject(project, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<List<TaskItemResponse>>.Fail(
                "Bạn không có quyền truy cập project này",
                ServiceErrorType.Forbidden
            );
        }

        var tasks = await _context.TaskItems
            .AsNoTracking()
            .Include(t => t.Project)
                .ThenInclude(p => p.Owner)
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.Id)
            .Select(t => new TaskItemResponse
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                DueDate = t.DueDate,
                ProjectId = t.ProjectId,
                ProjectName = t.Project.Name,
                OwnerId = t.Project.OwnerId,
                OwnerName = t.Project.Owner.Name
            })
            .ToListAsync();

        return ServiceResult<List<TaskItemResponse>>.Ok(
            tasks, "Lấy danh sách task thành công"
        );
    }

    public async Task<ServiceResult<TaskItemResponse>> GetTasksByIdAsync(int projectId, int taskId, int currentUserId, string currentUserRole)
    {
        var task = await _context.TaskItems
            .AsNoTracking()
            .Include(t => t.Project)
                .ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

        if (task == null)
        {
            return ServiceResult<TaskItemResponse>.Fail(
                "Không tìm thấy task.", ServiceErrorType.NotFound
            );
        }

        if (CanAccessProject(task.Project, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<TaskItemResponse>.Fail(
                "Bạn không có quyền truy cập task này", ServiceErrorType.Forbidden
            );
        }

        return ServiceResult<TaskItemResponse>.Ok(
            MapToTaskItemResponse(task), "Lấy thông tin task thành công"
        );
    }

    public async Task<ServiceResult<TaskItemResponse>> CreateTaskAsync(int projectId, CreateTaskItemRequest request, int currentUserId, string currentUserRole)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return ServiceResult<TaskItemResponse>.Fail(
                "Không tìm thấy project.",
                ServiceErrorType.NotFound
            );
        }

        if (CanAccessProject(project, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<TaskItemResponse>.Fail(
                "Bạn không có quyền tạo task trong project này", ServiceErrorType.Forbidden
            );
        }

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            Status = TaskItemStatus.Todo,
            ProjectId = projectId
        };

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} created task {TaskId} in project {ProjectId}",
            currentUserId,
            task.Id,
            projectId
        );

        var createdTask = await _context.TaskItems
            .AsNoTracking()
            .Include(t => t.Project)
                .ThenInclude(p => p.Owner)
            .FirstAsync(t => t.Id == task.Id);

        return ServiceResult<TaskItemResponse>.Ok(
            MapToTaskItemResponse(createdTask), "Tạo task thành công"
        );
    }

    public async Task<ServiceResult<TaskItemResponse>> UpdateTaskAsync(int projectId, int taskId, UpdateTaskItemRequest request, int currentUserId, string currentUserRole)
    {
        var task = await _context.TaskItems
            .Include(t => t.Project)
                .ThenInclude(p => p.Owner)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

        if (task == null)
        {
            return ServiceResult<TaskItemResponse>.Fail(
                "Không tìm thấy task", ServiceErrorType.NotFound
            );
        }

        if (CanAccessProject(task.Project, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<TaskItemResponse>.Fail(
                "Bạn không có quyền cập nhật task này", ServiceErrorType.Forbidden
            );
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.Status = request.Status;
        task.DueDate = request.DueDate;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "user {UserId} updated task {TaskId}",
            currentUserId,
            taskId
        );

        return ServiceResult<TaskItemResponse>.Ok(
            MapToTaskItemResponse(task), "Cập nhật task thành công"
        );
    }

    public async Task<ServiceResult<bool>> DeleteTaskAsync(int projectId, int taskId, int currentUserId, string currentUserRole)
    {
        var task = await _context.TaskItems
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

        if (task == null)
        {
            return ServiceResult<bool>.Fail(
                "Không tìm thấy task", ServiceErrorType.NotFound
            );
        }

        if (CanAccessProject(task.Project, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<bool>.Fail(
                "Bạn không có quyền xóa task này", ServiceErrorType.Forbidden
            );
        }

        task.Deleted = true;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} soft deleted task {TaskId}",
            currentUserId,
            task.Id
        );

        return ServiceResult<bool>.Ok(
            true, "Xóa task thành công"
        );
    }

}