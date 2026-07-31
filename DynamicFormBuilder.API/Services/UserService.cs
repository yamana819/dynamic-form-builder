using DynamicFormBuilder.API.Models;
using DynamicFormBuilder.API.Data;
using Microsoft.AspNetCore.Identity;
using DynamicFormBuilder.API.DTOs.User;
using DynamicFormBuilder.API.Constants;
using Microsoft.EntityFrameworkCore;

namespace DynamicFormBuilder.API.Services;


public class UserService : IUserService
{
    //Databaseden verileri çekmek için _context fieldi oluşturuyoruz.
    private readonly DynamicFormBuilderDbContext _context;
    //Şifre hashleme işlemleri için _passwordHasher fieldi oluşturuyoruz.
    private readonly IPasswordHasher<User> _passwordHasher;
    public UserService(DynamicFormBuilderDbContext context,IPasswordHasher<User> passwordHasher)
    {
        _context=context;
        _passwordHasher=passwordHasher;
    }
    //--2--
    // Buradaki private metodlarımızı DTO larımız ve Entitylerimiz arasında mappingleme yapmak için yazdık.
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
    private void AdminUpdateEntityFromDto(User user,AdminUserUpdateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.UserName))
        {
            user.UserName=dto.UserName;
        }
        if (dto.RoleId.HasValue)
        {
            user.RoleId=dto.RoleId.Value;
        }
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash=_passwordHasher.HashPassword(user,dto.Password);
        }
        if (dto.IsDeleted.HasValue)
        {
            user.IsDeleted=dto.IsDeleted.Value;
        }
    }
    private void UpdateEntityFromDto(User user,UserUpdateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.UserName))
        {
            user.UserName=dto.UserName;
        }
    }

    private bool ChangePasswordFromDto(User user,UserChangePasswordDto dto)
    {
        var vertificationResult = _passwordHasher.VerifyHashedPassword(user,user.PasswordHash,dto.CurrentPassword);
        if (vertificationResult == PasswordVerificationResult.Failed)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return false;
        }
        user.PasswordHash=_passwordHasher.HashPassword(user,dto.Password);
        return true;
    }
    public async Task<UserResponseDto?> GetUserAsync(Guid userId)
    {
        User? user = await _context.Users
                    .Include(u=>u.Role)
                    .Where(u=>u.UserId==userId && !u.IsDeleted)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }
        return MapToDto(user);
    }

    public async Task<UserResponseDto> CreateUserAsync(UserCreateDto userInfo)
    {
        User dummyUser = new User();
        string hashedPassword = _passwordHasher.HashPassword(dummyUser,userInfo.Password);
        User user = MapToUser(userInfo,hashedPassword);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        await _context.Entry(user).Reference(u=>u.Role).LoadAsync();
        return MapToDto(user);
    }

    public async Task<UserResponseDto?> UpdateUserAsync(Guid userId,UserUpdateDto dto)
    {
        User? user = await _context.Users
                    .Include(u=>u.Role)
                    .Where(u=> u.UserId==userId && !u.IsDeleted)
                    .FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }
        UpdateEntityFromDto(user,dto);
        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        User? user = await _context.Users
                    .Where(u=> u.UserId == userId && !u.IsDeleted)
                    .FirstOrDefaultAsync();
        if (user == null)
        {
            return false;
        }
        user.IsDeleted=true;
        await _context.SaveChangesAsync();
        return true;
    }
     public async Task<IEnumerable<AdminUserResponseDto>> GetAllUsersAsync(int pageNumber=1,int pageSize=50)
    {
        return await _context.Users
                .Where(u=> !u.IsDeleted)
                .OrderBy(u=>u.UserId)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .Select(u=>new AdminUserResponseDto
                {
                    UserId=u.UserId,
                    UserName=u.UserName,
                    RoleId=u.RoleId,
                    RoleName=u.Role.RoleName,
                    UserLastActiveDate=u.UserLastActiveDate,
                    UserStartDate=u.UserStartDate,
                    IsDeleted=u.IsDeleted
                })
                .AsNoTracking()
                .ToListAsync();
    }
    public async Task<AdminUserResponseDto?> GetUserForAdminAsync(Guid userId)
    {
        User? user = await _context.Users
                .Include(u=>u.Role)
                .Where(u=>u.UserId==userId)
                .FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }
        return MapToAdminDto(user);
    }
    public async Task<AdminUserResponseDto?> UpdateUserForAdminAsync(Guid userId,AdminUserUpdateDto dto)
    {
        User? user = await _context.Users
                    .Where(u=>u.UserId==userId)
                    .Include(u=>u.Role)
                    .FirstOrDefaultAsync();
        if (user == null)
        {
            return null;
        }
        AdminUpdateEntityFromDto(user,dto);
        await _context.SaveChangesAsync();
        return MapToAdminDto(user);
    }
    public async Task<bool> ChangePasswordAsync(Guid userId,UserChangePasswordDto dto)
    {
        User? user = await _context.Users
                    .Where(u=>u.UserId==userId && !u.IsDeleted)
                    .FirstOrDefaultAsync();
        if (user == null)
        {
            return false;
        }
        if (!ChangePasswordFromDto(user, dto))
        {
            return false;
        }
        await _context.SaveChangesAsync();
        return true;
    }
}