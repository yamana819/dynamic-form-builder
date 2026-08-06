using DynamicFormBuilder.API.DTOs.FormGroup;


namespace DynamicFormBuilder.API.Services;

public interface IFormGroupService
{
    Task<IEnumerable<FormGroupResponseDto>> GetAllFormGroupsAsync(int pageNumber=1,int pageSize=50);

    Task<FormGroupResponseDto> GetFormGroupAsync(string FormGroupCode);

    Task<FormGroupResponseDto> CreateFormGroupAsync(FormGroupCreateDto dto);

    Task<FormGroupResponseDto> UpdateFormGroupAsync(string FormGroupCode,FormGroupUpdateDto dto);

    Task DeleteFormGroupAsync(string FormGroupCode);
}