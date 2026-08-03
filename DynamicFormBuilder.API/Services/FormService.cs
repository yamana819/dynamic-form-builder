using DynamicFormBuilder.API.Models;
using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.DTOs.Form;

namespace DynamicFormBuilder.API.Services;


public class FormService : IFormService
{
    private readonly DynamicFormBuilderDbContext _context;

    public FormService(DynamicFormBuilderDbContext context)
    {
        _context=context;
    }
    private Form MapToForm(FormCreateDto dto)
    {
        return new Form
        {
            FormName=dto.FormName,
            FormGroupId=dto.FormGroupId,

        };
    }

    public Task<IEnumerable<FormResponseDto>> GetFormsByGroupAsync(Guid formGroupId)
    {
        
    }
}