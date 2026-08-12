using System.Data;
using DynamicFormBuilder.API.Exceptions;
using DynamicFormBuilder.API.Services;
using DynamicFormBuilder.API.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DynamicFormBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecordController:BaseApiController
    {
        private readonly IRecordService _recordService;
        private readonly IPermissionService _permissionService;
        public RecordController(IRecordService recordService,IPermissionService permissionService,IUserService userService):base(userService)
        {
            _permissionService=permissionService;
            _recordService=recordService;
        }
        [HttpGet("{formId}")]
        public async Task<IActionResult> GetAllRecords(Guid formId)
        {
            var roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanView);
            var result = await _recordService.GetAllRecordsAsync(formId);
            return Ok(result);
        }
        [HttpGet("{formId}/records/{recordId}")]
        public async Task<IActionResult> GetRecordById(Guid formId, object recordId)
        {
            var roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanView);
            var result =  await _recordService.GetRecordByIdAsync(formId,recordId) ?? throw new ResourceNotFoundException("Kayıt bulunamadı.");
            if (result.Values==null)
            {
                throw new ResourceNotFoundException("Kayıt bulunamadı ya da silinmiş.");
            }
            return Ok(result);
        }
        [HttpPost("{formId}")]
        public async Task<IActionResult> InsertRecord(Guid formId,Dictionary<string,object> formData)
        {
            var roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanCreate);
            var result = await _recordService.InsertRecordAsync(formId,formData);
            if (result == 0)
            {
                throw new BadRequestException("Kayıt eklenirken bir hata oluştu");
            }
            return StatusCode(201,result);
        }
        [HttpPatch("{formId}/records/{recordId}")]
        public async Task<IActionResult> UpdateRecord(Guid formId, object recordId, Dictionary<string, object> formData)
        {
            var roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanEdit);
            var result = await _recordService.UpdateRecordAsync(formId,recordId,formData);
            if (result == 0)
            {
                throw new ResourceNotFoundException("Güncellenmek istenen kayıt bulunamadı ya da silinmiş.");
            }
            return Ok(result);
        }
        [HttpDelete("{formId}/records/{recordId}")]
        public async Task<IActionResult> DeleteRecord(Guid formId, object recordId)
        {
            var roleId = await GetCurrentUserRoleIdAsync();
            await _permissionService.CheckPermissionForFormAsync(roleId,formId,PermissionType.CanDelete);
            var result = await _recordService.DeleteRecordAsync(formId,recordId);
            if (result == 0)
            {
                throw new ResourceNotFoundException("Silmek istenen kayıt bulunamadı ya da silinmiş.");
            }
            return NoContent();
        }
    }
}