using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;
using DynamicFormBuilder.API.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using DynamicFormBuilder.API.Filters;
using DynamicFormBuilder.API.Constants;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseApiController
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService):base(userService)
        {
            _userService=userService;
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            Guid userId = await GetCurrentUserIdAsync();
            UserResponseDto user = await _userService.GetUserAsync(userId);
            return Ok(user);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser(UserCreateDto dto)
        {
            UserResponseDto createdUser = await _userService.CreateUserAsync(dto);
            return StatusCode(201,createdUser);
        }
        [Authorize]
        [HttpPatch("me")]
        public async Task<IActionResult> UpdateMe(UserUpdateDto dto)
        {
            Guid userId = await GetCurrentUserIdAsync();
            UserResponseDto user = await _userService.UpdateUserAsync(userId,dto);
            return Ok(user);
        }
        [Authorize]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe()
        {
            Guid userId = await GetCurrentUserIdAsync();
            await _userService.DeleteUserAsync(userId);
            return NoContent();
        }
        [Authorize]
        [HttpPatch("me/change-password")]
        public async Task<IActionResult> ChangePassword(UserChangePasswordDto dto)
        {
            Guid userId = await GetCurrentUserIdAsync();
            await _userService.ChangePasswordAsync(userId,dto);
            return NoContent();
        }
        [RequirePermission("/admin/users",PermissionType.CanView)]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber=1,[FromQuery] int pageSize=50)
        {
            var users = await _userService.GetAllUsersAsync(pageNumber,pageSize);
            return Ok(users);
        }
        [RequirePermission("/admin/users",PermissionType.CanView)]
        [HttpGet("admin/{id}")]
        public async Task<IActionResult> GetUserForAdmin(Guid id)
        {
            AdminUserResponseDto user = await _userService.GetUserForAdminAsync(id);
            return Ok(user);
        }
        [HttpPatch("admin/{id}")]
        [RequirePermission("/admin/users",PermissionType.CanEdit)]
        public async Task<IActionResult> UpdateUserForAdmin(Guid id,AdminUserUpdateDto dto)
        {
            AdminUserResponseDto user = await _userService.UpdateUserForAdminAsync(id,dto);
            return Ok(user);
        }
    }
}