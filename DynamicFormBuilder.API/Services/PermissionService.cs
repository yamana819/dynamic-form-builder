using DynamicFormBuilder.API.Constants;
using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.Exceptions;
using Microsoft.EntityFrameworkCore;




namespace DynamicFormBuilder.API.Services;


public class PermissionService:IPermissionService
{
    private readonly DynamicFormBuilderDbContext _context;
    
    public PermissionService(DynamicFormBuilderDbContext context)
    {
        _context=context;
    }
    public async Task CheckPermissionForFormGroupAsync(byte roleId,string formGroupCode,PermissionType permissionType)
    {
        var auth = await _context.Authorizations
                        .Where(a=>a.RoleId==roleId && a.Menu.Href == $"/forms/{formGroupCode}")
                        .FirstOrDefaultAsync() ?? throw new ForbiddenException("Bu işlem için yetkiniz yok.");
        
        bool hasPermission=false;
        hasPermission = permissionType switch
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
    public async Task CheckPermissionForFormAsync(byte roleId,Guid formId,PermissionType permissionType)
    {
        var formGroupCode= await _context.Forms
                                .Where(f=>f.FormId==formId)
                                .Select(f=>f.FormGroupCode)
                                .AsNoTracking()
                                .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("İzin kontrülü sırasında form grubu bulunamadı");
        var auth = await _context.Authorizations
                        .Where(a=>a.RoleId==roleId && a.Menu.Href == $"/forms/{formGroupCode}/{formId}")
                        .FirstOrDefaultAsync() ?? throw new ForbiddenException("Bu işlem için yetkiniz yok.");
        
        bool hasPermission=false;
        hasPermission = permissionType switch
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
}