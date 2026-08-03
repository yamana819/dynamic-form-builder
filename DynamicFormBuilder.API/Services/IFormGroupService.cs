using DynamicFormBuilder.API.DTOs.FormGroup;


namespace DynamicFormBuilder.API.Services;

public interface IFormGroupService
{
    Task<IEnumerable<FormGroupResponseDto>> GetAllFormGroupsAsync(int pageNumber=1,int pageSize=50);

    Task<FormGroupResponseDto> GetFormGroupAsync(Guid formGroupId);

    Task<FormGroupResponseDto> CreateFormGroupAsync(FormGroupCreateDto dto);

    Task<FormGroupResponseDto> UpdateFormGroupAsync(Guid formGroupId,FormGroupUpdateDto dto);

    Task DeleteFormGroupAsync(Guid formGroupId);
}