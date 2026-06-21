using UserApi.DTOs;
using UserApi.Models;

namespace UserApi.Services.Interfaces;

public interface IUserService
{
    Task<PageResult<User>> GetUsersAsync(UserQueryParameters parameters);
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(CreateUserRequest request);
    Task<User> UpdateUserAsync(int id,UpdateUserRequest request);
    Task SoftDeleteUserAsync(int id);

}