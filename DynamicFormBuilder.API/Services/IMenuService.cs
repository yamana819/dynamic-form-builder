




using DynamicFormBuilder.API.DTOs.Menu;

namespace DynamicFormBuilder.API.Services;


public interface IMenuService
{
    Task<IEnumerable<MenuResponseDto>> GetMenusByRoleIdAsync(byte roleId);

    Task CreateMenuForFormGroupAsync(string formGroupCode,string formGroupName);

    Task CreateMenuForFormAsync(Guid formId,string formGroupCode,string formName);

    Task UpdateMenuForFormGroupAsync(string formGroupCode,string newName);

    Task UpdateMenuForFormAsync(Guid formId,string formGroupCode,string newName);
}