using Microsoft.EntityFrameworkCore;
using UserApi.Common;
using UserApi.Data;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Models;
using UserApi.Services.Interfaces;

namespace UserApi.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectService> _logger;
    public ProjectService(AppDbContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static bool CanAccessProject(Project project, int currentUserId, string currentUserRole)
    {
        return currentUserRole == "Admin" || project.OwnerId == currentUserId;
    }

    private static ProjectResponse MapToProjectResponse(Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            OwnerName = project.Owner.Name
        };
    }

    public async Task<ServiceResult<List<ProjectResponse>>> GetProjectsAsync(int currentUserId, string currentUserRole)
    {
        var query = _context.Projects
            .AsNoTracking()
            .Include(p => p.Owner)
            .AsQueryable();

        if (currentUserRole != "Admin")
        {
            query = query.Where(p => p.OwnerId == currentUserId);
        }

        var projects = await query
            .OrderByDescending(p => p.Id)
            .Select(p => new ProjectResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                OwnerId = p.OwnerId,
                OwnerName = p.Owner.Name
            }).ToListAsync();

        return ServiceResult<List<ProjectResponse>>.Ok(
            projects, "Lấy danh sách project thành công "
        );
    }

    public async Task<ServiceResult<ProjectResponse>> GetProjectByIdAsync(int id, int currentUserId, string currentUserRole)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return ServiceResult<ProjectResponse>.Fail(
                "Không tìm thấy project",
                ServiceErrorType.NotFound
            );
        }

        if (CanAccessProject(project, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<ProjectResponse>.Fail(
                "Bạn không có quyền truy cập",
                ServiceErrorType.Forbidden
            );
        }

        return ServiceResult<ProjectResponse>.Ok(
            MapToProjectResponse(project),
            "Lấy thông tin project thành công"
        );
    }

    public async Task<ServiceResult<ProjectResponse>> CreateProjectAsync(CreateProjectRequest request, int currentUserId)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = currentUserId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} created project {ProjectId}", currentUserId, project.Id
        );

        var createdProject = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Owner)
            .FirstAsync(p => p.Id == project.Id);

        return ServiceResult<ProjectResponse>.Ok(
            MapToProjectResponse(createdProject),
            "Tạo project thành công"
        );
    }

    public async Task<ServiceResult<ProjectResponse>> UpdateProjectAsync(int id, UpdateProjectRequest request, int currentUserId, string currentUserRole)
    {
        var project = await _context.Projects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return ServiceResult<ProjectResponse>.Fail(
                "Không tìm thấy project",
                ServiceErrorType.NotFound
            );
        }

        if (CanAccessProject(project, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<ProjectResponse>.Fail(
                "Bạn không có quyền cập nhật project này",
                ServiceErrorType.Forbidden
            );
        }

        project.Name = request.Name;
        project.Description = request.Description;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} updated project{ProjectId}", currentUserId, project.Id
        );

        return ServiceResult<ProjectResponse>.Ok(
            MapToProjectResponse(project),
            "Cập nhật project thành công"
        );
    }

    public async Task<ServiceResult<bool>> DeleteProjectAsync(int id, int currentUserId, string currentUserRole)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
        {
            return ServiceResult<bool>.Fail(
                "Không tìm thấy project",
                ServiceErrorType.NotFound
            );
        }

        if (CanAccessProject(project, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<bool>.Fail(
                "Bạn không có quyền xóa project này",
                ServiceErrorType.Forbidden
            );
        }

        project.Deleted = true;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} soft deleted project {ProjectId}", currentUserId, project.Id
        );

        return ServiceResult<bool>.Ok(
            true, "Xóa project thành công"
        );
    }

}