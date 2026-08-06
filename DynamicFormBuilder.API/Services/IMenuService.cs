




using DynamicFormBuilder.API.DTOs.Menu;

namespace DynamicFormBuilder.API.Services;


public interface IMenuService
{
    Task<IEnumerable<MenuResponseDto>> GetMenusByRoleIdAsync(byte roleId);

    Task CreateMenuForFormGroupAsync(string formGroupCode,string formGroupName,byte creatorRoleId);

    Task CreateMenuForFormAsync(Guid formId,string formGroupCode,string formName,byte creatorRoleId);

    Task UpdateMenuForFormGroupAsync(string formGroupCode,string newName);

    Task UpdateMenuForFormAsync(Guid formId,string formGroupCode,string newName);

    Task DeleteMenuForFormGroupAsync(string formGroupCode);
    
    Task DeleteMenuForFormAsync(Guid formId, string formGroupCode);
}