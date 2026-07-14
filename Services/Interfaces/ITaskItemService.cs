using UserApi.Common;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;

namespace UserApi.Services.Interfaces;

public interface ITaskItemService
{
    Task<ServiceResult<List<TaskItemResponse>>> GetTasksByProjectAsync(int projectId, int currentUserId, string currentUserRole);
    Task<ServiceResult<TaskItemResponse>> GetTasksByIdAsync(int projectId, int taskId, int currentUserId, string currentUserRole);
    Task<ServiceResult<TaskItemResponse>> CreateTaskAsync(int projectId, CreateTaskItemRequest request, int currentUserId, string currentUserRole);
    Task<ServiceResult<TaskItemResponse>> UpdateTaskAsync(int projectId, int taskId, UpdateTaskItemRequest request, int currentUserId, string currentUserRole);
    Task<ServiceResult<bool>> DeleteTaskAsync(int projectId, int taskId, int currentUserId, string currentUserRole);
}