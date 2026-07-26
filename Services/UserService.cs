using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserApi.Common;
using UserApi.Data;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Models;
using UserApi.Services.Interfaces;
namespace UserApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Description = user.Description,
            Age = user.Age,
            Role = user.Role
        };
    }

    private static bool IsAdmin(string currentUserRole)
    {
        return currentUserRole == "Admin";
    }

    private static bool CanAccessUser(int targetUserId, int currentUserId, string currentUserRole)
    {
        return IsAdmin(currentUserRole) == true || targetUserId == currentUserId;
    }

    public async Task<ServiceResult<UserResponse>> GetUserByIdAsync(int id, int currentUserId, string currentUserRole)
    {
        if (CanAccessUser(id, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<UserResponse>.Fail(
                "Bạn không có quyền xem thông tin người dùng này", ServiceErrorType.Forbidden
            );
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.Deleted == false);

        if (user == null)
        {
            return ServiceResult<UserResponse>.Fail(
                $"Không tìm thấy người dùng với id: {id} ",
                ServiceErrorType.NotFound
            );

        }

        var response = MapToUserResponse(user);

        return ServiceResult<UserResponse>.Ok(
            response,
            "Lấy thông tin user thành công"
        );
    }

    public async Task<ServiceResult<PageResult<UserResponse>>> GetUsersAsync(UserQueryParameters parameters, string currentUserRole)
    {
        // kiểm tra người dùng có role là admin
        if (IsAdmin(currentUserRole) == false)
        {
            return ServiceResult<PageResult<UserResponse>>.Fail(
                "Bạn không có quyền xem danh sách người dùng", ServiceErrorType.Forbidden
            );
        }

        // b1: lấy tất cả records có trong bảng users với Deleted = false
        var query = _context.Users
            .AsNoTracking()
            .Where(u => u.Deleted == false)
            .AsQueryable();

        // b2: áp dụng tìm kiếm theo tên nếu có, ngược lại trống hoặc khoảng trắng thì lấy tất cả
        if (string.IsNullOrWhiteSpace(parameters.SearchTerm) == false)
        {
            var keyword = parameters.SearchTerm.Trim();
            query = query.Where(u => u.Name.Contains(keyword));
        }

        // b3: áp dụng sắp xếp tăng hoặc giảm nếu có.Nếu nhập sai sortBy thì dùng Id làm mặc định, ngược lại mặc định sắp xếp theo Id,Name hoặc CreatedAt.
        query = parameters.SortBy?.ToLower() switch
        {
            "name" => parameters.Desc
                ? query.OrderByDescending(u => u.Name)
                : query.OrderBy(u => u.Name),

            "age" => parameters.Desc
                ? query.OrderByDescending(u => u.Age)
                : query.OrderBy(u => u.Age),

            "createdat" => parameters.Desc
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),

            _ => parameters.Desc
                ? query.OrderByDescending(u => u.Id)
                : query.OrderBy(u => u.Id)
        };

        // tính tổng số bản ghi
        var totalItem = await query.CountAsync();

        // b4: áp dụng phân trang
        var users = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Description = u.Description,
                Age = u.Age
            })
            .ToListAsync();

        var result = new PageResult<UserResponse>
        {
            Items = users,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalItems = totalItem,
            TotalPages = (int)Math.Ceiling((double)totalItem / parameters.PageSize)
        };

        return ServiceResult<PageResult<UserResponse>>.Ok(
            result,
            "Lấy danh sách user thành công"
        );
    }

    public async Task<ServiceResult<UserResponse>> CreateUserAsync(CreateUserRequest request)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email && u.Deleted == false);

        if (emailExists)
        {
            // ghi lại log theo hành động nghiệp vụ, thao tác bị từ chối do lỗi email trùng
            _logger.LogWarning("Create user rejected because email already exists");
            return ServiceResult<UserResponse>.Fail(
                "Email đã tồn tại",
                ServiceErrorType.Conflict
            );
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Age = request.Age,
            // CreatedAt = DateTime.UtcNow,
            // UpdatedAt = DateTime.UtcNow,
            // Deleted = false
        };

        var passwordHasher = new PasswordHasher<User>();
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "create user {UserId}", user.Id
        );

        var response = MapToUserResponse(user);

        return ServiceResult<UserResponse>.Ok(
            response,
            "Tạo user mới thành công"
        );
    }

    public async Task<ServiceResult<UserResponse>> UpdateUserAsync(int id, UpdateUserRequest request, int currentUserId, string currentUserRole)
    {
        if (CanAccessUser(id, currentUserId, currentUserRole) == false)
        {
            return ServiceResult<UserResponse>.Fail(
                "Bạn không có quyền cập nhật người dùng này", ServiceErrorType.Forbidden
            );
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Deleted == false);

        if (user == null)
        {
            // ghi lại log theo hành động nghiệp vụ, thao tác bị từ chối do không tìm thấy người dùng
            _logger.LogWarning("Update user rejected because user don't exist");
            return ServiceResult<UserResponse>.Fail(
                $"Không tìm thấy user với id: {id}",
                ServiceErrorType.NotFound
            );
        }

        var emailExists = await _context.Users
            .AnyAsync(
                u => u.Email == request.Email &&
                u.Id != id &&
                u.Deleted == false
            );

        if (emailExists)
        {
            // ghi lại log theo hành động nghiệp vụ, thao tác bị từ chối do lỗi email trùng
            _logger.LogWarning("Update user rejected because email already exists");
            return ServiceResult<UserResponse>.Fail(
                "Email đã tồn tại",
                ServiceErrorType.Conflict
            );
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.Description = request.Description;
        user.Age = request.Age;
        // user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId}", user.Id);

        var response = MapToUserResponse(user);

        return ServiceResult<UserResponse>.Ok(
            response,
            "Cập nhật user thành công"
        );
    }

    public async Task<ServiceResult<bool>> SoftDeleteUserAsync(int id, string currentUserRole)
    {
        if (IsAdmin(currentUserRole) == false)
        {
            return ServiceResult<bool>.Fail(
                "Bạn không có quyền xóa người dùng", ServiceErrorType.Forbidden
            );
        }
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Deleted == false);

        if (user == null)
        {
            _logger.LogWarning("Update user rejected because user don't exist");
            return ServiceResult<bool>.Fail(
                $"Không tìm thấy user với id: {id}",
                ServiceErrorType.NotFound
            );
        }

        user.Deleted = true;
        // user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Soft deleted user {UserId}", user.Id);

        return ServiceResult<bool>.Ok(
            true,
            "Xóa user thành công"
        );
    }

    public async Task<ServiceResult<UserResponse>> UpdateUserRoleAsync(int id, UpdateUserRoleRequest request)
    {
        var allowedRoles = new[] { "User", "Admin" };
        if (allowedRoles.Contains(request.Role) == false)
        {
            return ServiceResult<UserResponse>.Fail(
                "Role không hợp lệ", ServiceErrorType.BadRequest
            );
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Deleted == false);

        if (user == null)
        {
            return ServiceResult<UserResponse>.Fail(
                "Không tìm thấy người dùng", ServiceErrorType.NotFound
            );
        }

        user.Role = request.Role;

        await _context.SaveChangesAsync();

        return ServiceResult<UserResponse>.Ok(
            MapToUserResponse(user), "Cập nhật role người dùng thành công"
        );
    }

    public async Task<ServiceResult<bool>> RestoreUserIsDeleted(int id, string currentUserRole)
    {
        if (IsAdmin(currentUserRole) == false)
        {
            return ServiceResult<bool>.Fail(
                "Bạn không có quyền xóa người dùng", ServiceErrorType.Forbidden
            );
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Deleted == true);

        if (user == null)
        {
            _logger.LogWarning("Update user rejected because user don't exist");
            return ServiceResult<bool>.Fail(
                $"Không tìm thấy user với id: {id}",
                ServiceErrorType.NotFound
            );
        }

        user.Deleted = false;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Restore deleted user {UserId}", user.Id);

        return ServiceResult<bool>.Ok(
            true,
            "Khôi phục user bị xóa thành công"
        );

    }
}