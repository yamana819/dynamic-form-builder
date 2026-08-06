




using DynamicFormBuilder.API.DTOs.Menu;

namespace DynamicFormBuilder.API.Services;


public interface IMenuService
{
    Task<IEnumerable<MenuResponseDto>> GetMenusByRoleIdAsync(byte roleId);

    Task<> CreateMenuAsync(Guid id,Guid parentId,string href,string menuName,);
}