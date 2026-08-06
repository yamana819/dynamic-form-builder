using DynamicFormBuilder.API.DTOs.Form;


namespace DynamicFormBuilder.API.Services;


public interface IFormService
{
    Task<IEnumerable<FormResponseDto>> GetFormsByGroupAsync(string FormGroupCode,int pageNumber=1,int pageSize=50);
    
    Task<FormResponseDto> GetFormAsync(Guid formId);

    Task<FormResponseDto> CreateFormAsync(FormCreateDto dto);

    Task<FormResponseDto> UpdateFormAsync(Guid formId,FormUpdateDto dto);

    Task DeleteFormAsync(Guid formId);

    Task<FormResponseDto> PublishFormAsync(Guid formId);
}