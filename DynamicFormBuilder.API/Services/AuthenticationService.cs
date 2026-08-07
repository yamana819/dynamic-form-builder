using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.DTOs;
using DynamicFormBuilder.API.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DynamicFormBuilder.API.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DynamicFormBuilder.API.Services;


public class AuthenticationService:IAuthenticationService
{
    private readonly DynamicFormBuilderDbContext _context;
    private readonly IConfiguration _configuration;

    private readonly PasswordHasher<User> _passwordHasher;

    public AuthenticationService(DynamicFormBuilderDbContext context,IConfiguration configuration,PasswordHasher<User> passwordHasher)
    {
        _context=context;
        _configuration=configuration;
        _passwordHasher=passwordHasher;
    }
    public async Task<AuthenticationResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users
                    .Where(u=>u.UserName==dto.UserName)
                    .AsNoTracking()
                    .FirstOrDefaultAsync() ?? throw new AuthenticationFailedException("Kullanıcı adı veya şifre yanlış.");
        if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password)==PasswordVerificationResult.Failed)
        {
            throw new AuthenticationFailedException("Kullanıcı adı veya şifre yanlış");
        }
        var claims = new[]
        {
            new Claim(ClaimTypes.Name,user.UserName),
            new Claim(ClaimTypes.Role,user.RoleId.ToString()),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
        var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
        var tokenOptions = new JwtSecurityToken(
            issuer:_configuration["JwtSettings:Issuer"],
            audience:_configuration["JwtSettings:Audience"],
            claims:claims,
            expires:DateTime.Now.AddHours(2),
            signingCredentials:creds
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return new AuthenticationResponseDto{Token=tokenString};
    }
}