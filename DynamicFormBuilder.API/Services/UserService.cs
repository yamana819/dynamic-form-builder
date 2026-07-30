using DynamicFormBuilder.API.Models;
using DynamicFormBuilder.API.Data;
using Microsoft.AspNetCore.Identity;
using DynamicFormBuilder.API.DTOs.User;
using DynamicFormBuilder.API.Constants;
using Microsoft.EntityFrameworkCore;

namespace DynamicFormBuilder.API.Services;


public class UserService : IUserService
{
    private readonly DynamicFormBuilderDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    public UserService(DynamicFormBuilderDbContext context,IPasswordHasher<User> passwordHasher)
    {
        _context=context;
        _passwordHasher=passwordHasher;
    }
    private User MapToUser(UserCreateDto dto,string hashedPassword)
    {
        return new User
        {
            UserName = dto.UserName,
            PasswordHash = hashedPassword,
            RoleId = DefaultValues.DefaultRoleId
        };
    }
    private UserResponseDto MapToDto(User user)
    {
        return new UserResponseDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            IsDeleted = user.IsDeleted,
            RoleName = user.Role.RoleName 
        };
    }
    private AdminUserResponseDto MapToAdminDto(User user)
    {
        return new AdminUserResponseDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            RoleId = user.RoleId,
            IsDeleted = user.IsDeleted,
            RoleName = user.Role.RoleName
        };
    }
    private void UpdateEntityFromDto(User user,UserUpdateDto updatedUser)
    {
        if (!string.IsNullOrWhiteSpace(updatedUser.UserName))
        {
            user.UserName=updatedUser.UserName;
        }
    }
    public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
    {
        return await _context.Users
                    .Include(u=>u.Role)
                    .Where(u=> !u.IsDeleted)
                    .AsNoTracking()
                    .Select(u=>new UserResponseDto
                    {
                        UserId = u.UserId,
                        UserName = u.UserName,
                        IsDeleted = u.IsDeleted,
                        RoleName = u.Role.RoleName
                    })
                    .ToListAsync();
    }
}