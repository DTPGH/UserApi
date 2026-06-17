using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.DTOs;
using UserApi.Models;
//using UserApi.Models;

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext appDbContext)
        {
            this._context = appDbContext;
        }

        // GET: api/User
        //lấy danh sách trong bảng User: Có lọc Deleted, paging, sorting và searching hoạt động đúng.
        [HttpGet("")]
        public async Task<ActionResult<ApiResponse<PageResult<User>>>> GetAll([FromQuery] UserQueryParameters queryParameters)
        {

            // kiểm tra nếu pageNumber nhỏ hơn 1 thì trả về lỗi
            if (queryParameters.PageNumber < 1)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Page number must be greater than 0",
                    Content = null
                });
            }

            // Giới hạn pageSize để tránh lấy quá nhiều dữ liệu.
            if (queryParameters.PageSize < 1 || queryParameters.PageSize > 100)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Page size must be between 1 and 100",
                    Content = null
                });
            }

            // b1: lấy dữ liệu từ database
            var query = _context.Users
                .AsNoTracking()
                .Where(u => u.Deleted == false);

            // b2: áp dụng tìm kiếm theo tên nếu có, ngược lại trống hoặc khoảng trắng thì lấy tất cả
            if (string.IsNullOrWhiteSpace(queryParameters.SearchTerm) == false)
            {
                query = query.Where(u => u.Name.Contains(queryParameters.SearchTerm));
            }

            // b3: áp dụng sắp xếp tăng hoặc giảm nếu có.Nếu nhập sai sortBy thì dùng Id làm mặc định, ngược lại mặc định sắp xếp theo Id,Name hoặc CreatedAt.
            bool isDescending = queryParameters.SortDirection == "desc";

            switch (queryParameters.SortBy?.ToLower())
            {
                case "name":
                    {
                        query = isDescending
                            ? query.OrderByDescending(u => u.Name)
                            : query.OrderBy(u => u.Name);
                    }
                    break;

                case "createdat":
                    {
                        query = isDescending
                            ? query.OrderByDescending(u => u.CreatedAt)
                            : query.OrderBy(u => u.CreatedAt);
                    }
                    break;

                default:
                    {
                        query = isDescending
                            ? query.OrderByDescending(u => u.Id)
                            : query.OrderBy(u => u.Id);
                    }
                    break;
            }

            // tính tổng số bản ghi
            var totalItem = await query.CountAsync();

            // b4: áp dụng phân trang
            var lstUser = await query
                .Skip((queryParameters.PageNumber - 1) * queryParameters.PageSize) // bỏ qua các bản ghi của trang trước
                .Take(queryParameters.PageSize) // lấy số bản ghi của trang hiện tại
                .ToListAsync();

            var pageResult = new PageResult<User>
            {
                TotalItems = totalItem,
                TotalPages = (int)Math.Ceiling((double)totalItem / queryParameters.PageSize),
                PageNumber = queryParameters.PageNumber,
                PageSize = queryParameters.PageSize,
                Items = lstUser
            };

            var response = new ApiResponse<PageResult<User>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Lấy danh sách người dùng thành công",
                Content = pageResult
            };
            return Ok(response);
        }

        // Lấy thông tin user chi tiết theo Id. Nếu không tìm thấy hoặc User đã bị xóa mềm thì trả về thông báo lỗi rõ ràng.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<User>>> GetUserById(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(
                    new ApiResponse<User>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = $"Not found a user with id: {id} in database ",
                        Content = null
                    }
                );
            }

            return Ok(new ApiResponse<User>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Get detail information user by id successfully",
                Content = user
            });

        }

        // POST: api/use
        // THêm mới user
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu đầu vào không hợp lệ",
                    Content = ModelState
                });
            }

            //nếu muốn email là duy nhất thì cần kiểm tra email trùng
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Deleted == false);

            if (emailExists)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Email này đã tồn tại ",
                    Content = null
                });
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

            return Ok(new ApiResponse<User>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Tạo user mới thành công",
                Content = user
            });
        }

        // PUT: api/user/1
        // Cập nhật user theo id 
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<User>>> UpdateUser([FromRoute] int id, [FromBody] UpdateUserRequest request)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Dữ liệu cập nhật không hợp lệ",
                    Content = ModelState
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id & u.Deleted == false);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Không tìm thấy user cần cập nhật",
                    Content = null
                });
            }

            var emailExists = await _context.Users
                .AnyAsync(
                    e => e.Email == request.Email &&
                    e.Id != id &&
                    !e.Deleted
                    );

            if (emailExists)
            {
                return BadRequest(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Email này đã tồn tại",
                    Content = null
                });
            }

            user.Name = request.Name;
            user.Email = request.Email;
            user.Description = request.Description;
            user.Age = request.Age;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new ApiResponse<User>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Cập nhật user thành công",
                Content = user
            });
        }

        // Xóa User bằng cách cập nhật Deleted thành true và cập nhật UpdatedAt.
        [HttpPost("{id:int}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Deleted==false);

            if(user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                   StatusCode = StatusCodes.Status404NotFound,
                   Message = "Không tìm thấy user cần xóa",
                   Content = null 
                });
            }

            user.Deleted = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Xóa mềm user thành công",
                Content = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Deleted,
                    user.UpdatedAt
                }
            });
        }
    }
}