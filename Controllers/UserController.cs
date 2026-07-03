using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserApi.Common;
using UserApi.Data;
using UserApi.DTOs.Requests;
using UserApi.DTOs.Responses;
using UserApi.Extensions;
using UserApi.Models;
using UserApi.Services;
using UserApi.Services.Interfaces;
//using UserApi.Models;

namespace UserApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
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

            return this.ToActionResult(pageResult);
        }

        // Lấy thông tin user chi tiết theo Id. Nếu không tìm thấy hoặc User đã bị xóa mềm thì trả về thông báo lỗi rõ ràng.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            return this.ToActionResult(user);

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

            return this.ToActionResult(user);
        }

        // Xóa User bằng cách cập nhật Deleted thành true và cập nhật UpdatedAt.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
        {

            var result = await _userService.SoftDeleteUserAsync(id);
            return this.ToActionResult(result);
        }
    }
} 