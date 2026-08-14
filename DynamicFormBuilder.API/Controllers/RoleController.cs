using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;
using DynamicFormBuilder.API.DTOs;
using DynamicFormBuilder.API.DTOs.Role;
using DynamicFormBuilder.API.Filters;
using DynamicFormBuilder.API.Constants;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        
        public RoleController(IRoleService roleService)
        {
            _roleService=roleService;
        }
        [HttpGet]
        [RequirePermission("/admin/authorizations",PermissionType.CanView)]
        public async Task<IActionResult> GetAllRoles([FromQuery] int pageNumber=1,[FromQuery] int pageSize=50)
        {
            IEnumerable<RoleResponseDto> roles = await _roleService.GetAllRolesAsync(pageNumber,pageSize);
            return Ok(roles);
        }
        [HttpGet("{id}")]
        [RequirePermission("/admin/authorizations",PermissionType.CanView)]
        public async Task<IActionResult> GetRoleAsync(byte id)
        {
            RoleResponseDto role = await _roleService.GetRoleAsync(id);
            return Ok(role);
        }
        [HttpPost]
        [RequirePermission("/admin/authorizations",PermissionType.CanCreate)]
        public async Task<IActionResult> CreateRoleAsync(RoleCreateDto dto)
        {
            RoleResponseDto role = await _roleService.CreateRoleAsync(dto);
            return StatusCode(201,role);
        }
        [HttpPatch("{id}")]
        [RequirePermission("/admin/authorizations",PermissionType.CanEdit)]
        public async Task<IActionResult> UpdateRoleAsync(byte id,RoleUpdateDto dto)
        {
            RoleResponseDto role = await _roleService.UpdateRoleAsync(id,dto);
            return Ok(role);
        }
        [HttpDelete("{id}")]
        [RequirePermission("/admin/authorizations",PermissionType.CanDelete)]
        public async Task<IActionResult> DeleteRoleAsync(byte id)
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }
    }
}