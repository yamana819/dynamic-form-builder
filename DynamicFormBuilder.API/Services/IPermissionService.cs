using DynamicFormBuilder.API.Constants;

namespace DynamicFormBuilder.API.Services;


public interface IPermissionService
{
    Task CheckPermissionForFormGroupAsync(byte roleId,string formGroupCode,PermissionType permissionType);

    Task CheckPermissionForFormAsync(byte roleId,Guid formId,PermissionType permissionType);
}