using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;
using DynamicFormBuilder.API.DTOs.User;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService=userService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            UserResponseDto user = await _userService.GetUserAsync(id);
            return Ok(user);
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserCreateDto dto)
        {
            UserResponseDto createdUser = await _userService.CreateUserAsync(dto);
            return StatusCode(201,createdUser);
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id,UserUpdateDto dto)
        {
            UserResponseDto user = await _userService.UpdateUserAsync(id,dto);
            return Ok(user);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
        [HttpPatch("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(Guid id,UserChangePasswordDto dto)
        {
            await _userService.ChangePasswordAsync(id,dto);
            return NoContent();
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber,[FromQuery] int pageSize)
        {
            var users = await _userService.GetAllUsersAsync(pageNumber,pageSize);
            return Ok(users);
        }
        [HttpGet("{id}/admin")]
        public async Task<IActionResult> GetUserForAdmin(Guid id)
        {
            AdminUserResponseDto user = await _userService.GetUserForAdminAsync(id);
            return Ok(user);
        }
        [HttpPatch("{id}/admin")]
        public async Task<IActionResult> UpdateUserForAdmin(Guid id,AdminUserUpdateDto dto)
        {
            AdminUserResponseDto user = await _userService.UpdateUserForAdminAsync(id,dto);
            return Ok(user);
        }
    }
}