using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.DTOs.FormGroup;
using DynamicFormBuilder.API.Models;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;
using DynamicFormBuilder.API.Constants;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class FormGroupController : ControllerBase
    {
        private readonly IFormGroupService _formGroupService;
        private readonly IPermissionService _permissionService;
        public FormGroupController(IFormGroupService formGroupService,IPermissionService permissionService)
        {
            _formGroupService=formGroupService;
            _permissionService=permissionService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllFormGroups()
        {
            var formGroups = await _formGroupService.GetAllFormGroupsAsync();
            return Ok(formGroups);
        }
        [HttpGet("{groupCode}")]
        public async Task<IActionResult> GetFormGroup(string groupCode)
        {
            await _permissionService.CheckPermissionForFormGroupAsync(roleId,groupCode,PermissionType.CanView);
            FormGroupResponseDto formGroup = await _formGroupService.GetFormGroupAsync(groupCode);
            return Ok(formGroup);
        }
        [HttpPost]
        public async Task<IActionResult> CreateFormGroup(FormGroupCreateDto dto)
        {
            await _permissionService.CheckPermissionForFormGroupAsync(roleId,dto.FormGroupCode,PermissionType.CanCreate);
            FormGroupResponseDto formGroup = await _formGroupService.CreateFormGroupAsync(dto);
            return StatusCode(201,formGroup);
        }
        [HttpPatch("{groupCode}")]
        public async Task<IActionResult> UpdateFormGroup(string groupCode,FormGroupUpdateDto dto)
        {
            await _permissionService.CheckPermissionForFormGroupAsync(roleId,groupCode,PermissionType.CanEdit);
            FormGroupResponseDto formGroup = await _formGroupService.UpdateFormGroupAsync(groupCode,dto);
            return Ok(formGroup);
        }
        [HttpDelete("{groupCode}")]
        public async Task<IActionResult> DeleteFormGroup(string groupCode)
        {
            await _permissionService.CheckPermissionForFormGroupAsync(roleId,groupCode,PermissionType.CanDelete);
            await _formGroupService.DeleteFormGroupAsync(groupCode);
            return NoContent();
        }
    }
}