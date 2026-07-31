using DynamicFormBuilder.API.DTOs.User;

namespace DynamicFormBuilder.API.Services;


public interface IUserService
{
    Task<IEnumerable<AdminUserResponseDto>> GetAllUsersAsync(int pageNumber=1,int pageSize=50);

    Task<AdminUserResponseDto> GetUserForAdminAsync(Guid userId);

    Task<AdminUserResponseDto> UpdateUserForAdminAsync(Guid userId,AdminUserUpdateDto userInfo);

    Task<UserResponseDto> GetUserAsync(Guid userId);

    Task<UserResponseDto> CreateUserAsync(UserCreateDto dto);

    Task<UserResponseDto> UpdateUserAsync(Guid userId,UserUpdateDto dto);

    Task DeleteUserAsync(Guid userId);

    Task ChangePasswordAsync(Guid userId,UserChangePasswordDto dto);
}