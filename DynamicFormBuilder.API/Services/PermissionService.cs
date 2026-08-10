using DynamicFormBuilder.API.Constants;
using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using DynamicFormBuilder.API.Models;



namespace DynamicFormBuilder.API.Services;


public class PermissionService:IPermissionService
{
    private readonly DynamicFormBuilderDbContext _context;

    private void ValidatePermission(Authorization auth, PermissionType permissionType)
    {
        bool hasPermission = permissionType switch
        {
            PermissionType.CanCreate => auth.CanCreate,
            PermissionType.CanEdit => auth.CanEdit,
            PermissionType.CanView => auth.CanView,
            PermissionType.CanDelete => auth.CanDelete,
            _ => false,
        };

        if (!hasPermission)
        {
            throw new ForbiddenException("Bu işlem için yetkiniz yok.");
        }
    }
    public PermissionService(DynamicFormBuilderDbContext context)
    {
        _context=context;
    }
    public async Task CheckPermissionAsync(byte roleId,string href,PermissionType permissionType)
    {
        var auth = await _context.Authorizations
                        .Where(a=>a.RoleId==roleId && a.Menu.Href == href)
                        .FirstOrDefaultAsync() ?? throw new ForbiddenException("Bu işlem için yetkiniz yok.");
        
        ValidatePermission(auth,permissionType);
    }
    public async Task CheckPermissionForFormAsync(byte roleId,Guid formId,PermissionType permissionType)
    {
        var formGroupCode= await _context.Forms
                                .Where(f=>f.FormId==formId)
                                .Select(f=>f.FormGroupCode)
                                .AsNoTracking()
                                .FirstOrDefaultAsync() ?? throw new ForbiddenException("Bu işlem için yetkiniz yok.");
        var auth = await _context.Authorizations
                        .Where(a=>a.RoleId==roleId && a.Menu.Href == $"/forms/{formGroupCode}/{formId}")
                        .FirstOrDefaultAsync() ?? throw new ForbiddenException("Bu işlem için yetkiniz yok.");
        
        ValidatePermission(auth,permissionType);
    }
}