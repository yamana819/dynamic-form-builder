using DynamicFormBuilder.API.DTOs.FormGroup;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;
using DynamicFormBuilder.API.Constants;
using DynamicFormBuilder.API.Filters;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormGroupController : BaseApiController
    {
        private readonly IFormGroupService _formGroupService;
        private readonly IPermissionService _permissionService;

        private readonly IMenuService _menuService;

        public FormGroupController(IFormGroupService formGroupService,IPermissionService permissionService,IMenuService menuService,IUserService userService):base(userService)
        {
            _formGroupService=formGroupService;
            _permissionService=permissionService;
            _menuService=menuService;
        }
        [HttpGet]
        [RequirePermission("/forms",PermissionType.CanView)]
        public async Task<IActionResult> GetAllFormGroups([FromQuery]int pageNumber=1,[FromQuery]int pageSize=50)
        {
            var formGroups = await _formGroupService.GetAllFormGroupsAsync(pageNumber,pageSize);
            return Ok(formGroups);
        }
        [HttpGet("{groupCode}")]
        public async Task<IActionResult> GetFormGroup(string groupCode)
        {
            string href = _menuService.BuildHrefForFormGroup(groupCode);
            byte roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionAsync(roleId,href,PermissionType.CanView);
            FormGroupResponseDto formGroup = await _formGroupService.GetFormGroupAsync(groupCode);
            return Ok(formGroup);
        }
        [HttpPost]
        [RequirePermission("/forms",PermissionType.CanCreate)]
        public async Task<IActionResult> CreateFormGroup(FormGroupCreateDto dto)
        {
            byte roleId = await GetCurrentUserRoleIdAsync();
            FormGroupResponseDto formGroup = await _formGroupService.CreateFormGroupAsync(dto,roleId);
            return StatusCode(201,formGroup);
        }
        [HttpPatch("{groupCode}")]
        public async Task<IActionResult> UpdateFormGroup(string groupCode,FormGroupUpdateDto dto)
        {
            string href = _menuService.BuildHrefForFormGroup(groupCode);
            byte roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionAsync(roleId,href,PermissionType.CanEdit);
            FormGroupResponseDto formGroup = await _formGroupService.UpdateFormGroupAsync(groupCode,dto);
            return Ok(formGroup);
        }
        [HttpDelete("{groupCode}")]
        public async Task<IActionResult> DeleteFormGroup(string groupCode)
        {
            string href = _menuService.BuildHrefForFormGroup(groupCode);
            byte roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionAsync(roleId,href,PermissionType.CanDelete);
            await _formGroupService.DeleteFormGroupAsync(groupCode);
            return NoContent();
        }
    }
}