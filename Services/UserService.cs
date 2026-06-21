using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.DTOs;
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

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && u.Deleted == false);
    }

    public async Task<PageResult<User>> GetUsersAsync(UserQueryParameters parameters)
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
            .ToListAsync();

        return new PageResult<User>
        {
            Items = users,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize,
            TotalItems = totalItem,
            TotalPages = (int)Math.Ceiling((double)totalItem / parameters.PageSize)
        };
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
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
        return user;
    }

    public async Task<User> UpdateUserAsync(int id, UpdateUserRequest request)
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
        return user;
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