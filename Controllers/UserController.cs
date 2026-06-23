using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.Common;
using UserApi.Data;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
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
        public async Task<ActionResult<ApiResponse<PageResult<UserResponse>>>> GetAll([FromQuery] UserQueryParameters queryParameters)
        {
            var pageResult = await _userService.GetUsersAsync(queryParameters);
            if (pageResult.Success == false || pageResult.Data is null)
            {
                return BadRequest(new ApiResponse<PageResult<UserResponse>>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = pageResult.Message
                });
            }

            var response = new ApiResponse<PageResult<UserResponse>>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = pageResult.Message,
                Content = pageResult.Data
            };
            return Ok(response);
        }

        // Lấy thông tin user chi tiết theo Id. Nếu không tìm thấy hoặc User đã bị xóa mềm thì trả về thông báo lỗi rõ ràng.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user.Success == false || user.Data == null)
            {
                if (user.ErrorType == ServiceErrorType.NotFound)
                {
                    return NotFound(
                    new ApiResponse<UserResponse>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = user.Message
                    });
                }
                return BadRequest(new ApiResponse<UserResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = user.Message,
                });
            }

            return Ok(new ApiResponse<UserResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = user.Message,
                Content = user.Data
            });

        }

        // POST: api/use
        // THêm mới user
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser([FromBody] CreateUserRequest request)
        {

            var user = await _userService.CreateUserAsync(request);
            if (user.Success == false || user.Data is null)
            {
                return Conflict(new ApiResponse<UserResponse>
                {
                    StatusCode = StatusCodes.Status409Conflict,
                    Message = user.Message
                });
            }

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.Data.Id },
                new ApiResponse<UserResponse>
                {
                    StatusCode = StatusCodes.Status201Created,
                    Message = user.Message,
                    Content = user.Data
                }
            );

        }

        // PUT: api/user/1
        // Cập nhật user theo id 
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(
            [FromRoute] int id,
            [FromBody] UpdateUserRequest request)
        {

            var user = await _userService.UpdateUserAsync(id, request);

            if (user.Success == false || user.Data is null)
            {
                if (user.ErrorType == ServiceErrorType.NotFound)
                {
                    return NotFound(new ApiResponse<UserResponse>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = user.Message
                    });
                }

                if (user.ErrorType == ServiceErrorType.Conflict)
                {
                    return Conflict(new ApiResponse<UserResponse>
                    {
                        StatusCode = StatusCodes.Status409Conflict,
                        Message = user.Message
                    });
                }

                return BadRequest(new ApiResponse<UserResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = user.Message
                });
            }

            return Ok(new ApiResponse<UserResponse>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = user.Message,
                Content = user.Data
            });
        }

        // Xóa User bằng cách cập nhật Deleted thành true và cập nhật UpdatedAt.
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteUser(int id)
        {

            var result = await _userService.SoftDeleteUserAsync(id);
            if (result.Success == false)
            {
                if (result.ErrorType == ServiceErrorType.NotFound)
                {
                    return NotFound(new ApiResponse<UserResponse>
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = result.Message
                    });
                }

                return BadRequest(new ApiResponse<UserResponse>
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = result.Message
                });
            }
            return Ok(new ApiResponse<object>
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result.Message
            });
        }
    }
}