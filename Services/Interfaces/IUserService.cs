using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Models;

namespace UserApi.Services.Interfaces;

public interface IUserService
{
    Task<PageResult<UserResponse>> GetUsersAsync(UserQueryParameters parameters);
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request);
    Task SoftDeleteUserAsync(int id);

}