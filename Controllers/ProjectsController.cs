using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Extensions;
using UserApi.Services.Interfaces;

namespace UserApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId)
            ? userId
            : null;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProjectResponse>>>> GetProjects()
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<List<ProjectResponse>>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _projectService.GetProjectsAsync(currentUserId.Value, currentUserRole);
        return this.ToActionResult(result);

    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> GetProjectById(int id)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<ProjectResponse>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _projectService.GetProjectByIdAsync(id, currentUserId.Value, currentUserRole);

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> CreateProject([FromBody] CreateProjectRequest request)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<ProjectResponse>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _projectService.CreateProjectAsync(request, currentUserId.Value);
        return this.ToActionResult(result, StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProjectResponse>>> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();
        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<ProjectResponse>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }
        var result = await _projectService.UpdateProjectAsync(id, request, currentUserId.Value, currentUserRole);
        return this.ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(int id)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();
        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<bool>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _projectService.DeleteProjectAsync(id, currentUserId.Value, currentUserRole);
        return this.ToActionResult(result);
    }
}