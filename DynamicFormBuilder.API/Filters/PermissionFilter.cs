using System.Security.Claims;
using DynamicFormBuilder.API.Constants;
using DynamicFormBuilder.API.Exceptions;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;


namespace DynamicFormBuilder.API.Filters;

public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string href, PermissionType permission)
        : base(typeof(PermissionFilter))
    {
        Arguments = new object[] { href, permission };
    }
}

public class PermissionFilter : IAsyncAuthorizationFilter
{
    private readonly string _href;
    private readonly PermissionType _permission;
    private readonly IPermissionService _permissionService;
    private readonly IUserService _userService;

    public PermissionFilter(string href, PermissionType permission,
        IPermissionService permissionService, IUserService userService)
    {
        _href = href;
        _permission = permission;
        _permissionService = permissionService;
        _userService = userService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
        {
            throw new AuthenticationFailedException("Kullanıcı kimliği doğrulanamadı.");
        }

        if (!Guid.TryParse(userIdClaim,out Guid userId))
        {
            throw new AuthenticationFailedException("Kullanıcı kimliği doğrulanamadı.");
        }
        var roleId = await _userService.GetRoleIdAsync(userId);
        await _permissionService.CheckPermissionAsync(roleId, _href, _permission);
    }
}