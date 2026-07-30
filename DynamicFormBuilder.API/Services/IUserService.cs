using DynamicFormBuilder.API.DTOs.User;

namespace DynamicFormBuilder.API.Services;


public interface IUserService
{
    Task<IEnumerable<AdminUserResponseDto>> GetAllUsersAsync();

    Task<AdminUserResponseDto?> GetUserForAdminAsync(Guid userId);

    Task<AdminUserResponseDto> UpdateUserForAdminAsync(Guid userId,AdminUserUpdateDto userInfo);

    Task<UserResponseDto> GetUserAsync(Guid userId);

    Task<UserResponseDto> CreateUserAsync(UserCreateDto userInfo);

    Task<UserResponseDto> UpdateUserAsync(Guid userId,UserUpdateDto userInfo);

    Task DeleteUserAsync(Guid userId);

    Task ChangePasswordAsync(Guid userId,UserChangePasswordDto dto);
}