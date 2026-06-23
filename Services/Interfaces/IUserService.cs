using UserApi.Common;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Models;

namespace UserApi.Services.Interfaces;

public interface IUserService
{
    Task<ServiceResult<PageResult<UserResponse>>> GetUsersAsync(UserQueryParameters parameters);
    Task<ServiceResult<UserResponse>> GetUserByIdAsync(int id);
    Task<ServiceResult<UserResponse>> CreateUserAsync(CreateUserRequest request);
    Task<ServiceResult<UserResponse>> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<ServiceResult<object>> SoftDeleteUserAsync(int id);

}