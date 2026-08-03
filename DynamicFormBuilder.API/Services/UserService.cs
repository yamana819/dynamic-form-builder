using DynamicFormBuilder.API.Models;
using DynamicFormBuilder.API.Data;
using Microsoft.AspNetCore.Identity;
using DynamicFormBuilder.API.DTOs.User;
using DynamicFormBuilder.API.Constants;
using Microsoft.EntityFrameworkCore;
using DynamicFormBuilder.API.Exceptions;

namespace DynamicFormBuilder.API.Services;


public class UserService : IUserService
{
    //Databaseden verileri çekmek için _context fieldi oluşturuyoruz.
    private readonly DynamicFormBuilderDbContext _context;
    //Şifre hashleme işlemleri için PasswordHassher objemizi tutacak _passwordHasher fieldi oluşturuyoruz.
    private readonly IPasswordHasher<User> _passwordHasher;
    public UserService(DynamicFormBuilderDbContext context,IPasswordHasher<User> passwordHasher)
    {
        _context=context;
        _passwordHasher=passwordHasher;
    }
    // Buradaki private metodlarımızı DTO larımız ve Entitylerimiz arasında mappingleme yapmak için yazdık.
    //UserCreateDto objesi olarak oluşturulan yeni kullanıcıyı User sınıfına mapliyoruz.
    private User MapToUser(UserCreateDto dto,string hashedPassword)
    {
        return new User
        {
            UserName = dto.UserName,
            PasswordHash = hashedPassword,
            RoleId = DefaultValues.DefaultRoleId
        };
    }
    //Frontende döndüreceğimiz bilgiler için user objesini User sınıfından UserResponseDtoya mapleyen metod.
    private UserResponseDto MapToDto(User user)
    {
        return new UserResponseDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            RoleName = user.Role.RoleName 
        };
    }
    //Admin panelinde döndüreceğimiz kullanıcı bilgileri için AdminUserResponseDtoya mapleyen metod.
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
    //Admin kullanıcı üzerinde güncelleme yaparken gönderilen dtoyu Usera mapleyen metod.
    //Güncelleme sırasında patch mantığı kullanacağımız için hepsine if kontrolü eklememiz gerekli.
    private void AdminUpdateEntityFromDto(User user,AdminUserUpdateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.UserName) && (user.UserName!=dto.UserName))
        {
            user.UserName=dto.UserName;
        }
        if (dto.RoleId.HasValue && (user.RoleId!=dto.RoleId))
        {
            user.RoleId=dto.RoleId.Value;
        }
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash=_passwordHasher.HashPassword(user,dto.Password);
        }
        if (dto.IsDeleted.HasValue && (user.IsDeleted!=dto.IsDeleted))
        {
            user.IsDeleted=dto.IsDeleted.Value;
        }
    }
    //Kullanıcı bilgilerini güncellerken gönderilen dtoyu Usera mapleyen metod.
    private void UpdateEntityFromDto(User user,UserUpdateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.UserName) && (user.UserName!=dto.UserName))
        {
            user.UserName=dto.UserName;
        }
    }
    //Şifre değişikliği sırasında gönderilen dtoyu kullanarak şifreyi değiştiren mapping metodu.
    private void ChangePasswordFromDto(User user,UserChangePasswordDto dto)
    {
        var vertificationResult = _passwordHasher.VerifyHashedPassword(user,user.PasswordHash,dto.CurrentPassword);
        if (vertificationResult == PasswordVerificationResult.Failed)
        {
            throw new BadRequestException("Eski şifrenizi yanlış girildi.");
        }
        user.PasswordHash=_passwordHasher.HashPassword(user,dto.Password);
    }
    //======================================== 1 Normal Kullanıcı için CRUD işlemleri ===================================================
    public async Task<UserResponseDto> GetUserAsync(Guid userId)
    {
        User? user = await _context.Users
                    .Include(u => u.Role)//UserResponseDtomuzda RoleName alanı olduğu için ilgili kullanıcının RoleIdsinin Role tablosuna referansını da çekiyoruz.
                    .Where(u => u.UserId == userId && !u.IsDeleted)
                    .AsNoTracking()//Sadece okuma yapacağımız değişiklik yapmayacağımız için performansı artırmak amaçlı AsNoTracking eklendi.
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Kullanıcı bulunamadı.");
        return MapToDto(user);
    }
    public async Task<UserResponseDto> CreateUserAsync(UserCreateDto dto)
    {
        bool userExists = await _context.Users.AnyAsync(u => u.UserName == dto.UserName);//Kullanıcı adının zaten var olup olmadığının kontrolü.
        if (userExists)
        {
            throw new ConflictException($"'{dto.UserName}' kullanıcı adı zaten kullanılıyor.");
        }
        User dummyUser = new User();
        string hashedPassword = _passwordHasher.HashPassword(dummyUser,dto.Password);
        User user = MapToUser(dto,hashedPassword);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        await _context.Entry(user).Reference(u=>u.Role).LoadAsync();
        return MapToDto(user);//Kullanıcıyı veritabanına kaydettikten sonra ResponseDtosu ile yeni kullanıcının bilgilerini de döndürüyoruz.
    }

    public async Task<UserResponseDto> UpdateUserAsync(Guid userId,UserUpdateDto dto)
    {
        User? user = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.UserId == userId && !u.IsDeleted)
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Güncelleme işlemi sırasında kullanıcı bulunamadı.");
        if (!string.IsNullOrEmpty(dto.UserName) && user.UserName != dto.UserName)
        {
            bool userExists = await _context.Users.AnyAsync(u=>u.UserName==dto.UserName);
            if (userExists)
            {
                throw new ConflictException($"'{dto.UserName}' kullanıcı adı zaten kullanılıyor.");
            }
        }
        UpdateEntityFromDto(user,dto);
        await _context.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        User? user = await _context.Users
                    .Where(u=> u.UserId == userId && !u.IsDeleted)
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Silme işlemi sırasında kullanıcı bulunamadı.");
        user.IsDeleted=true;
        await _context.SaveChangesAsync();
    }
    // ======================================== 2 Admin için CRUD işlemleri ve şifre değiştirme ===================================================
     public async Task<IEnumerable<AdminUserResponseDto>> GetAllUsersAsync(int pageNumber=1,int pageSize=50)
    {
        return await _context.Users
                .OrderBy(u=>u.UserId)
                .Skip((pageNumber-1)*pageSize)
                .Take(pageSize)
                .Select(u=>new AdminUserResponseDto//Select yazıldığında RoleName yazıldığı anda zaten Role tablosundan referans geliyor bu yüzden Include() gereksiz olduğu için sildim.
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
                .ToListAsync();//Bütün kullanıcıları Selectle çektikten sonra AdminUserResponseDtolara bilgilerini yazıyoruz ve bir liste olarak dönüyoruz.
    }
    public async Task<AdminUserResponseDto> GetUserForAdminAsync(Guid userId)
    {
        User? user = await _context.Users
                .Include(u=>u.Role)
                .Where(u=>u.UserId==userId)
                .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Kullanıcı bulunamadı(ADMIN).");
        return MapToAdminDto(user);
    }
    public async Task<AdminUserResponseDto> UpdateUserForAdminAsync(Guid userId,AdminUserUpdateDto dto)
    {
        User? user = await _context.Users
                    .Where(u=>u.UserId==userId)
                    .Include(u=>u.Role)
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Güncelleme işlemi sırasında kullanıcı bulunamadı(ADMIN)");
        AdminUpdateEntityFromDto(user,dto);
        await _context.SaveChangesAsync();
        return MapToAdminDto(user);
    }
    public async Task ChangePasswordAsync(Guid userId,UserChangePasswordDto dto)
    {
        User? user = await _context.Users
                    .Where(u=>u.UserId==userId && !u.IsDeleted)
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Şifre değişikliği işlemi sırasında kullanıcı bulunamadı."); 
        ChangePasswordFromDto(user,dto);
        await _context.SaveChangesAsync();
    }
}