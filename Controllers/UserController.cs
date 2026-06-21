using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.Data;
using UserApi.DTOs;
using UserApi.Models;
using UserApi.Services;
using UserApi.Services.Interfaces;
//using UserApi.Models;

namespace UserApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            this._userService = userService;
        }

        // GET: api/User
        //lấy danh sách trong bảng User: Có lọc Deleted, paging, sorting và searching hoạt động đúng.
        [HttpGet("")]
        public async Task<ActionResult<ApiResponse<PageResult<User>>>> GetAll([FromQuery] UserQueryParameters queryParameters)
        {

            // // kiểm tra nếu pageNumber nhỏ hơn 1 thì trả về lỗi
            // if (queryParameters.PageNumber < 1)
            // {
            //     return BadRequest(new ApiResponse<string>
            //     {
            //         StatusCode = StatusCodes.Status400BadRequest,
            //         Message = "Page number must be greater than 0",
            //         Content = null
            //     });
            // }

            // // Giới hạn pageSize để tránh lấy quá nhiều dữ liệu.
            // if (queryParameters.PageSize < 1 || queryParameters.PageSize > 100)
            // {
            //     return BadRequest(new ApiResponse<string>
            //     {
            //         StatusCode = StatusCodes.Status400BadRequest,
            //         Message = "Page size must be between 1 and 100",
            //         Content = null
            //     });
            // }

            var pageResult = await _userService.GetUsersAsync(queryParameters);

            var message = pageResult.TotalItems == 0
                ? "Không tìm thấy user phù hợp"
                : "Lấy danh sách users thành công";

            var response = new ApiResponse<PageResult<User>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = message,
                Content = pageResult
            };
            return Ok(response);
        }

        // Lấy thông tin user chi tiết theo Id. Nếu không tìm thấy hoặc User đã bị xóa mềm thì trả về thông báo lỗi rõ ràng.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<User>>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

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
        public async Task<ActionResult<ApiResponse<User>>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                return CreatedAtAction(
                    nameof(GetUserById),
                    new { id = user.Id },
                    new ApiResponse<User>
                    {
                        StatusCode = StatusCodes.Status201Created,
                        Message = "Tạo user mới thành công",
                        Content = user
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<User>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message,
                    Content = null
                });
            }
        }

        // PUT: api/user/1
        // Cập nhật user theo id 
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<User>>> UpdateUser(
            [FromRoute] int id,
            [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, request);

                return Ok(new ApiResponse<User>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Cập nhật user thành công",
                    Content = user
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<User>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<User>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = ex.Message
                });
            }
        }

        // Xóa User bằng cách cập nhật Deleted thành true và cập nhật UpdatedAt.
        [HttpPost("{id:int}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                await _userService.SoftDeleteUserAsync(id);
                return Ok(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status200OK,
                    Message = "Xóa mềm user thành công",
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<object>
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = ex.Message
                });
            }

        }
    }
}