using UserApi.Common;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;

namespace UserApi.Services.Interfaces;

public interface IProjectService
{
    Task<ServiceResult<List<ProjectResponse>>> GetProjectsAsync(int currentUserId, string currentUserRole);
    Task<ServiceResult<ProjectResponse>> GetProjectByIdAsync(int id, int currentUserId, string currentUserRole);
    Task<ServiceResult<ProjectResponse>> CreateProjectAsync(CreateProjectRequest request, int currentUserId);
    Task<ServiceResult<ProjectResponse>> UpdateProjectAsync(int id, UpdateProjectRequest request, int currentUserId, string currentUserRole);
    Task<ServiceResult<bool>> DeleteProjectAsync(int id, int currentUserId, string currentUserRole);
}