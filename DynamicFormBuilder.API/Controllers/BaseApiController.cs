using System.Security.Claims;
using DynamicFormBuilder.API.Exceptions;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;



namespace DynamicFormBuilder.API.Controllers;
public abstract class BaseApiController : ControllerBase
{
    private readonly IUserService _userService;

    protected BaseApiController(IUserService userService)
    {
        _userService = userService;
    }

    protected async Task<byte> GetCurrentUserRoleIdAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new AuthenticationFailedException("Kullanıcı kimliği doğrulanamadı.");

        if (!Guid.TryParse(userIdClaim,out Guid userId))
        {
            throw new AuthenticationFailedException("Kullanıcı kimliği doğrulanamadı.");
        }
        return await _userService.GetRoleIdAsync(userId);
    }
    protected async Task<Guid> GetCurrentUserIdAsync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new AuthenticationFailedException("Kullanıcı kimliği doğrulanamadı.");
        if (!Guid.TryParse(userIdClaim,out Guid userId))
        {
            throw new AuthenticationFailedException("Kullanıcı kimliği doğrulanamadı.");
        }
        return userId;
    }
}