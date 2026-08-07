using DynamicFormBuilder.API.DTOs.Form;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;
using DynamicFormBuilder.API.Constants;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormController : ControllerBase
    {
        private readonly IFormService _formService;
        private readonly IPermissionService _permissionService;
        public FormController(IFormService formService,IPermissionService permissionService)
        {
            _formService=formService;
            _permissionService=permissionService;
        }
    
        [HttpGet("forms/{formGroupCode}")]
        public async Task<IActionResult> GetFormsByGroup(string formGroupCode)
        {
            await _permissionService.CheckPermissionForFormGroupAsync(roleId,formGroupCode,PermissionType.CanView);
            var forms = await _formService.GetFormsByGroupAsync(formGroupCode);
            return Ok(forms);
        }
        [HttpGet("{formId}")]
        public async Task<IActionResult> GetForm(Guid formId)
        {
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanView);
            FormResponseDto form = await _formService.GetFormAsync(formId);
            return Ok(form);
        }
        [HttpPost]
        public async Task<IActionResult> CreateForm(FormCreateDto dto)
        {
            await _permissionService.CheckPermissionForFormGroupAsync(roleId,dto.FormGroupCode,PermissionType.CanCreate);
            FormResponseDto form = await _formService.CreateFormAsync(dto);
            return StatusCode(201,form);
        }
        [HttpPatch("{formId}")]
        public async Task<IActionResult> UpdateForm(Guid formId,FormUpdateDto dto)
        {
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanEdit);
            FormResponseDto form = await _formService.UpdateFormAsync(formId,dto);
            return Ok(form);
        }
        [HttpPatch("publish-form/{formId}")]
        public async Task<IActionResult> PublishForm(Guid formId)
        {
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanEdit);
            FormResponseDto form = await _formService.PublishFormAsync(formId);
            return Ok(form);
        }

        [HttpDelete("{formId}")]
        public async Task<IActionResult> DeleteForm(Guid formId)
        {
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanDelete);
            await _formService.DeleteFormAsync(formId);
            return NoContent();
        }
    }
}