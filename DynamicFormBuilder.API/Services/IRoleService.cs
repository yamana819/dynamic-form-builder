


using DynamicFormBuilder.API.DTOs.Role;

namespace DynamicFormBuilder.API.Services;


public interface IRoleService
{
    Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync(int pageNumber=1,int pageSize=50);

    Task<RoleResponseDto> GetRoleAsync(byte roleId);

    Task<RoleResponseDto> CreateRoleAsync(RoleCreateDto dto);

    Task<RoleResponseDto> UpdateRoleAsync(byte roleId,RoleUpdateDto dto);

    Task DeleteRoleAsync(byte roleId);
}