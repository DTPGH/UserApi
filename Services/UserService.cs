using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Models;
using UserApi.Services.Interfaces;
namespace UserApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    public UserService(AppDbContext context)
    {
        _context = context;
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Description = user.Description,
            Age = user.Age
        };
    }

    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.Deleted == false);

        if (user == null)
        {
            return null;
        }
        return MapToUserResponse(user);
    }

    public async Task<PageResult<UserResponse>> GetUsersAsync(UserQueryParameters parameters)
    {
        // b1: lấy tất cả records có trong bảng users với Deleted = false
        var query = _context.Users
            .AsNoTracking()
            .Where(u => u.Deleted == false);

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

        return new PageResult<UserResponse>
        {
            Items = users,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalItems = totalItem,
            TotalPages = (int)Math.Ceiling((double)totalItem / parameters.PageSize)
        };
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email && u.Deleted == false);

        if (emailExists)
        {
            throw new InvalidOperationException("Email đã tồn tại");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            Age = request.Age,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Deleted = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return MapToUserResponse(user);
    }

    public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Deleted == false);

        if (user == null)
        {
            throw new KeyNotFoundException($"không tìm thấy user với id: {id}.");
        }

        var emailExists = await _context.Users
            .AnyAsync(
                u => u.Email == request.Email &&
                u.Id != id &&
                u.Deleted == false
            );

        if (emailExists)
        {
            throw new InvalidOperationException("Email đã tồn tại");
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.Description = request.Description;
        user.Age = request.Age;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToUserResponse(user);
    }
    public async Task SoftDeleteUserAsync(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.Deleted == false);

        if (user == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy user với id:{id}.");
        }

        user.Deleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}