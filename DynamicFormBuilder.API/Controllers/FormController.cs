using DynamicFormBuilder.API.DTOs.Form;
using DynamicFormBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormController : ControllerBase
    {
        private readonly IFormService _formService;
        public FormController(IFormService formService)
        {
            _formService=formService;
        }
    
        [HttpGet("{formGroupId}/forms-by-group")]
        public async Task<IActionResult> GetFormsByGroup(Guid formGroupId)
        {
            var forms = await _formService.GetFormsByGroupAsync(formGroupId);
            return Ok(forms);
        }
        [HttpGet("{formId}")]
        public async Task<IActionResult> GetForms(Guid formId)
        {
            FormResponseDto form = await _formService.GetFormAsync(formId);
            return Ok(form);
        }
        [HttpPost]
        public async Task<IActionResult> CreateForm(FormCreateDto dto)
        {
            FormResponseDto form = await _formService.CreateFormAsync(dto);
            return StatusCode(201,form);
        }
        [HttpPatch("{formId}")]
        public async Task<IActionResult> UpdateForm(Guid formId,FormUpdateDto dto)
        {
            FormResponseDto form = await _formService.UpdateFormAsync(formId,dto);
            return Ok(201);
        }
        [HttpDelete("{formId}")]
        public async Task<IActionResult> DeleteForm(Guid formId)
        {
            await _formService.DeleteFormAsync(formId);
            return NoContent();
        }
    }
}