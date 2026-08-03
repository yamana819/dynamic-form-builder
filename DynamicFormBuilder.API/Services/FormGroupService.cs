using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.DTOs.FormGroup;
using DynamicFormBuilder.API.Exceptions;
using DynamicFormBuilder.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicFormBuilder.API.Services;


public class FormGroupService : IFormGroupService
{
    private readonly DynamicFormBuilderDbContext _context;

    public FormGroupService(DynamicFormBuilderDbContext context)
    {
        _context=context;
    }
    //Private mapping metodlarını yazıyoruz.
    private FormGroup MapToFormGroup(FormGroupCreateDto dto)
    {
        return new FormGroup
        {
            FormGroupName=dto.FormGroupName
        };
    }
    private FormGroupResponseDto MapToDto(FormGroup form)
    {
        return new FormGroupResponseDto
        {
            FormGroupId=form.FormGroupId,
            FormGroupName=form.FormGroupName,
            CreatedAt=form.CreatedAt,
            LastUpdate=form.LastUpdate
        };
    }

    private void UpdateEntityFromDto(FormGroup formGroup,FormGroupUpdateDto dto)
    {
        if (!string.IsNullOrEmpty(dto.FormGroupName))
        {
            formGroup.FormGroupName=dto.FormGroupName;
            formGroup.LastUpdate=DateTime.UtcNow;
        }
    }

    public async Task<IEnumerable<FormGroupResponseDto>> GetAllFormGroupsAsync(int pageNumber=1,int pageSize=50)
    {
        return await _context.FormGroups
            .Where(f=>!f.IsDeleted)
            .OrderBy(f=>f.FormGroupId)
            .Skip((pageNumber-1)*pageSize)
            .Take(pageSize)
            .Select(f=>new FormGroupResponseDto
            {
                FormGroupId=f.FormGroupId,                
                FormGroupName=f.FormGroupName,
                CreatedAt=f.CreatedAt,
                LastUpdate=f.LastUpdate
            })
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<FormGroupResponseDto> GetFormGroupAsync(Guid formGroupId)
    {
        FormGroup? formGroup = await _context.FormGroups
                            .Where(f=>f.FormGroupId==formGroupId && !f.IsDeleted)
                            .AsNoTracking()
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form grubu bulunamadı.");
        return MapToDto(formGroup);
    }

    public async Task<FormGroupResponseDto> CreateFormGroupAsync(FormGroupCreateDto dto)
    {
        bool formgGroupExists = await _context.FormGroups.AnyAsync(f=>f.FormGroupName==dto.FormGroupName);
        if (formgGroupExists)
        {
            throw new ConflictException($"{dto.FormGroupName} form grubu ismi zaten kullanılıyor.");
        }
        FormGroup formGroup = MapToFormGroup(dto);
        _context.FormGroups.Add(formGroup);
        await _context.SaveChangesAsync();
        return MapToDto(formGroup);
    }
    
    public async Task<FormGroupResponseDto> UpdateFormGroupAsync(Guid formGroupId,FormGroupUpdateDto dto)
    {
        FormGroup? formGroup = await _context.FormGroups
                            .Where(f=>f.FormGroupId==formGroupId && !f.IsDeleted)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Güncelleme işlemi sırasında form grubu bulunamadı.");
        if (!string.IsNullOrEmpty(dto.FormGroupName) && dto.FormGroupName != formGroup.FormGroupName)
        {
            bool formGroupExists = await _context.FormGroups
                .AnyAsync(f => f.FormGroupName == dto.FormGroupName);
            if (formGroupExists)
            {
                throw new ConflictException($"'{dto.FormGroupName}' form grubu ismi zaten kullanılıyor.");
            }
        }
        UpdateEntityFromDto(formGroup,dto);
        await _context.SaveChangesAsync();
        return MapToDto(formGroup);
    }

    public async Task DeleteFormGroupAsync(Guid formGroupId)
    {
        FormGroup? formGroup = await _context.FormGroups
                            .Where(f=>f.FormGroupId==formGroupId && !f.IsDeleted)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Silme işlemi sırasında form grubu bulunamadı.");
        formGroup.IsDeleted=true;
        await _context.SaveChangesAsync();
    }
}