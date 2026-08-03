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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFormGroup(Guid id)
        {
            FormGroupResponseDto formGroup = await _formGroupService.GetFormGroupAsync(id);
            return Ok(formGroup);
        }
        [HttpPost]
        public async Task<IActionResult> CreateFormGroup(FormGroupCreateDto dto)
        {
            FormGroupResponseDto formGroup = await _formGroupService.CreateFormGroupAsync(dto);
            return StatusCode(201,formGroup);
        }
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateFormGroup(Guid id,FormGroupUpdateDto dto)
        {
            FormGroupResponseDto formGroup = await _formGroupService.UpdateFormGroupAsync(id,dto);
            return Ok(formGroup);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFormGroup(Guid id)
        {
            await _formGroupService.DeleteFormGroupAsync(id);
            return NoContent();
        }
    }
}