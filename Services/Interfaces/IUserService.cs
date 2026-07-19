using UserApi.Common;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Models;

namespace UserApi.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<PageResult<UserResponse>>> GetUsersAsync(UserQueryParameters parameters, string currentUserRole);
    Task<ServiceResult<UserResponse>> GetUserByIdAsync(int id, int currentUserId, string currentUserRole);
    Task<ServiceResult<UserResponse>> CreateUserAsync(CreateUserRequest request);
    Task<ServiceResult<UserResponse>> UpdateUserAsync(int id, UpdateUserRequest request, int currentUserId, string currentUserRole);
    Task<ServiceResult<bool>> SoftDeleteUserAsync(int id, string currentUserRole);
    Task<ServiceResult<UserResponse>> UpdateUserRoleAsync(int id, UpdateUserRoleRequest request);

}