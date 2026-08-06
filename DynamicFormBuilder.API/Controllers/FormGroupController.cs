using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.DTOs.FormGroup;
using DynamicFormBuilder.API.Models;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class FormGroupController : ControllerBase
    {
        private readonly IFormGroupService _formGroupService;
        public FormGroupController(IFormGroupService formGroupService)
        {
            _formGroupService=formGroupService;
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
            FormGroupResponseDto formGroup = await _formGroupService.GetFormGroupAsync(groupCode);
            return Ok(formGroup);
        }
        [HttpPost]
        public async Task<IActionResult> CreateFormGroup(FormGroupCreateDto dto)
        {
            FormGroupResponseDto formGroup = await _formGroupService.CreateFormGroupAsync(dto);
            return StatusCode(201,formGroup);
        }
        [HttpPatch("{groupCode}")]
        public async Task<IActionResult> UpdateFormGroup(string groupCode,FormGroupUpdateDto dto)
        {
            FormGroupResponseDto formGroup = await _formGroupService.UpdateFormGroupAsync(groupCode,dto);
            return Ok(formGroup);
        }
        [HttpDelete("{groupCode}")]
        public async Task<IActionResult> DeleteFormGroup(string groupCode)
        {
            await _formGroupService.DeleteFormGroupAsync(groupCode);
            return NoContent();
        }
    }
}