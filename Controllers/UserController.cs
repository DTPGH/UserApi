using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId)
                ? userId : null;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // GET: api/User
        // cập nhật rule: chỉ có admin mới có thể xem danh sách tất cả người dùng
        //lấy danh sách trong bảng User: Có lọc Deleted, paging, sorting và searching hoạt động đúng.
        [HttpGet("")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PageResult<UserResponse>>>> GetAll([FromQuery] UserQueryParameters queryParameters)
        {
            var currentUserRole = GetCurrentUserRole();

            var pageResult = await _userService.GetUsersAsync(queryParameters, currentUserRole);

            return this.ToActionResult(pageResult);
        }

        // cập nhật rule: chỉ có admin mới có thể xem bất kỳ người dùng nào theo id nhưng người dùng với role user thì chỉ có thể xem chính mình
        // Lấy thông tin user chi tiết theo Id. Nếu không tìm thấy hoặc User đã bị xóa mềm thì trả về thông báo lỗi rõ ràng.
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse<UserResponse>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Token không hợp lệ"
                });
            }

            var user = await _userService.GetUserByIdAsync(id, currentUserId.Value, currentUserRole);

            return this.ToActionResult(user);

        }

        // POST: api/use
        // cập nhật rule: chỉ có admin mới có thể dùng chức năng tạo người dùng mới 
        // THêm mới user
        [HttpPost]
        [Authorize(Roles = "Admin")]
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
        // cập nhật rule: chỉ có admin mới có thể cập nhật bất kỳ thông tin nào của người dùng theo id, nhưng với user thì là cập nhật tự chính mình
        // Cập nhật user theo id 
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(
            [FromRoute] int id,
            [FromBody] UpdateUserRequest request)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserRole = GetCurrentUserRole();

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse<UserResponse>
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Token không hợp lệ"
                });
            }

            var user = await _userService.UpdateUserAsync(id, request, currentUserId.Value, currentUserRole);

            return this.ToActionResult(user);
        }

        // cập nhật rule: chỉ có admin mới có thể xóa mềm
        // Xóa User bằng cách cập nhật Deleted thành true và cập nhật UpdatedAt.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(int id)
        {
            var currentUserRole = GetCurrentUserRole();

            var result = await _userService.SoftDeleteUserAsync(id, currentUserRole);
            return this.ToActionResult(result);
        }

        [HttpPatch("{id:int}/role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request)
        {
            var result = await _userService.UpdateUserRoleAsync(id, request);
            return this.ToActionResult(result);
        }

        [HttpPatch("{id:int}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> RestoreUserDeleted(int id)
        {
            var currentUserRole = GetCurrentUserRole();
            var result = await _userService.RestoreUserIsDeleted(id, currentUserRole);
            return this.ToActionResult(result);
        }

        [HttpGet("deleted")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetDeletedUsers()
        {
            var currentUserRole = GetCurrentUserRole();

            var result = await _userService.GetDeletedUsersAsync(currentUserRole);

            return this.ToActionResult(result);
        }
    }
}