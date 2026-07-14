using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Extensions;
using UserApi.Services.Interfaces;

namespace UserApi.Controllers;

[ApiController]
[Route("api/project/{projectId:int}/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskItemService _taskItemService;
    public TasksController(ITaskItemService taskItemService)
    {
        _taskItemService = taskItemService;
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
    public async Task<ActionResult<ApiResponse<List<TaskItemResponse>>>> GetTasks(int projectId)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<List<TaskItemResponse>>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _taskItemService.GetTasksByProjectAsync(
            projectId,
            currentUserId.Value,
            currentUserRole
        );
        return this.ToActionResult(result);
    }

    [HttpGet("{taskId:int}")]
    public async Task<ActionResult<ApiResponse<TaskItemResponse>>> GetTaskById(int projectId, int taskId)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<TaskItemResponse>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _taskItemService.GetTasksByIdAsync(
            projectId, taskId, currentUserId.Value, currentUserRole
        );

        return this.ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskItemResponse>>> CreateTask(int projectId, [FromBody] CreateTaskItemRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<TaskItemResponse>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _taskItemService.CreateTaskAsync(
            projectId, request, currentUserId.Value, currentUserRole
        );

        return this.ToActionResult(result, StatusCodes.Status201Created);
    }

    [HttpPut("{taskId:int}")]
    public async Task<ActionResult<ApiResponse<TaskItemResponse>>> UpdateTask(int projectId, int taskId, [FromBody] UpdateTaskItemRequest request)
    {
        var currentUserId = GetCurrentUserId();
        var currentUserRole = GetCurrentUserRole();

        if (currentUserId == null)
        {
            return Unauthorized(new ApiResponse<TaskItemResponse>
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Token không hợp lệ"
            });
        }

        var result = await _taskItemService.UpdateTaskAsync(
            projectId, taskId, request, currentUserId.Value, currentUserRole
        );

        return this.ToActionResult(result);
    }

    [HttpDelete("{taskId:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTask(int projectId, int taskId)
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

        var result = await _taskItemService.DeleteTaskAsync(
            projectId, taskId, currentUserId.Value, currentUserRole
        );

        return this.ToActionResult(result);
    }
}