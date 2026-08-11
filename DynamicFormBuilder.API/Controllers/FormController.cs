using DynamicFormBuilder.API.DTOs.Form;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;
using DynamicFormBuilder.API.Constants;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormController : BaseApiController
    {
        private readonly IFormService _formService;
        private readonly IPermissionService _permissionService;

        private readonly IMenuService _menuService;
        public FormController(IFormService formService,IPermissionService permissionService,IMenuService menuService,IUserService userService):base(userService)
        {
            _formService=formService;
            _permissionService=permissionService;
            _menuService=menuService;
        }
    
        [HttpGet("forms/{formGroupCode}")]
        public async Task<IActionResult> GetFormsByGroup(string formGroupCode,[FromQuery]int pageNumber=1,[FromQuery]int pageSize=50)
        {
            string href = _menuService.BuildHrefForFormGroup(formGroupCode);
            byte roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionAsync(roleId,href,PermissionType.CanView);
            var forms = await _formService.GetFormsByGroupAsync(formGroupCode,pageNumber,pageSize);
            return Ok(forms);
        }
        [HttpGet("{formId}")]
        public async Task<IActionResult> GetForm(Guid formId)
        {
            byte roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanView);
            FormResponseDto form = await _formService.GetFormAsync(formId);
            return Ok(form);
        }
        [HttpPost]
        public async Task<IActionResult> CreateForm(FormCreateDto dto)
        {
            string href = _menuService.BuildHrefForFormGroup(dto.FormGroupCode);
            byte roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionAsync(roleId,href,PermissionType.CanCreate);
            FormResponseDto form = await _formService.CreateFormAsync(dto,roleId);
            return StatusCode(201,form);
        }
        [HttpPatch("{formId}")]
        public async Task<IActionResult> UpdateForm(Guid formId,FormUpdateDto dto)
        {
            byte roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanEdit);
            FormResponseDto form = await _formService.UpdateFormAsync(formId,dto);
            return Ok(form);
        }
        [HttpPatch("publish-form/{formId}")]
        public async Task<IActionResult> PublishForm(Guid formId)
        {
            byte roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanEdit);
            FormResponseDto form = await _formService.PublishFormAsync(formId);
            return Ok(form);
        }

        [HttpDelete("{formId}")]
        public async Task<IActionResult> DeleteForm(Guid formId)
        {
            byte roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanDelete);
            await _formService.DeleteFormAsync(formId);
            return NoContent();
        }
    }
}