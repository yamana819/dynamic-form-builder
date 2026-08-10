using DynamicFormBuilder.API.Constants;

namespace DynamicFormBuilder.API.Services;


public interface IPermissionService
{
    Task CheckPermissionForFormAsync(byte roleId,Guid formId,PermissionType permissionType);

    Task CheckPermissionAsync(byte roleId,string href,PermissionType permissionType);
}