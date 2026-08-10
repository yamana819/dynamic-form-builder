using System.Text.RegularExpressions;
using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.DTOs.FormGroup;
using DynamicFormBuilder.API.Exceptions;
using DynamicFormBuilder.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicFormBuilder.API.Services;


public class FormGroupService : IFormGroupService
{
    private readonly DynamicFormBuilderDbContext _context;

    private readonly IMenuService _menuService;
    public FormGroupService(DynamicFormBuilderDbContext context,IMenuService menuService)
    {
        _context=context;
        _menuService=menuService;
    }
    //Private mapping metodlarını yazıyoruz.
    private FormGroup MapToFormGroup(FormGroupCreateDto dto)
    {
        return new FormGroup
        {
            FormGroupCode=dto.FormGroupCode,
            FormGroupName=Regex.Replace(dto.FormGroupName,@"\s+"," ").Trim()
        };
    }
    private FormGroupResponseDto MapToDto(FormGroup form)
    {
        return new FormGroupResponseDto
        {
            FormGroupCode=form.FormGroupCode,
            FormGroupName=form.FormGroupName,
            CreatedAt=form.CreatedAt,
            LastUpdate=form.LastUpdate
        };
    }

    private async Task UpdateEntityFromDto(FormGroup formGroup,FormGroupUpdateDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.FormGroupName) && (dto.FormGroupName!=formGroup.FormGroupName))
        {
            formGroup.FormGroupName=Regex.Replace(dto.FormGroupName,@"\s+"," ").Trim();
            await _menuService.UpdateMenuForFormGroupAsync(formGroup.FormGroupCode,formGroup.FormGroupName);
            formGroup.LastUpdate=DateTime.UtcNow;
        }
    }

    public async Task<IEnumerable<FormGroupResponseDto>> GetAllFormGroupsAsync(int pageNumber=1,int pageSize=50)
    {
        return await _context.FormGroups
            .Where(f=>!f.IsDeleted)
            .OrderBy(f=>f.FormGroupCode)
            .Skip((pageNumber-1)*pageSize)
            .Take(pageSize)
            .Select(f=>new FormGroupResponseDto
            {
                FormGroupCode=f.FormGroupCode,                
                FormGroupName=f.FormGroupName,
                CreatedAt=f.CreatedAt,
                LastUpdate=f.LastUpdate
            })
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<FormGroupResponseDto> GetFormGroupAsync(string formGroupCode)
    {
        FormGroup? formGroup = await _context.FormGroups
                            .Where(f=>f.FormGroupCode==formGroupCode && !f.IsDeleted)
                            .AsNoTracking()
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form grubu bulunamadı.");
        return MapToDto(formGroup);
    }

    public async Task<FormGroupResponseDto> CreateFormGroupAsync(FormGroupCreateDto dto,byte creatorRoleId)
    {
        bool formgGroupNameExists = await _context.FormGroups.AnyAsync(f=>f.FormGroupName==dto.FormGroupName);
        bool formGroupCodeExists = await _context.FormGroups.AnyAsync(f=>f.FormGroupCode==dto.FormGroupCode);
        if (formgGroupNameExists)
        {
            throw new ConflictException("Form grup ismi zaten kullanılıyor.");
        }
        if (formGroupCodeExists)
        {
            throw new ConflictException("Form grup kodu zaten kullanılıyor.");
        }
        FormGroup formGroup = MapToFormGroup(dto);
        await using var transaction =await _context.Database.BeginTransactionAsync();
        try
        {
            _context.FormGroups.Add(formGroup);
            await _context.SaveChangesAsync();
            await _menuService.CreateMenuForFormGroupAsync(formGroup.FormGroupCode,formGroup.FormGroupName,creatorRoleId);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        return MapToDto(formGroup);
    }
    
    public async Task<FormGroupResponseDto> UpdateFormGroupAsync(string formGroupCode,FormGroupUpdateDto dto)
    {
        FormGroup? formGroup = await _context.FormGroups
                            .Where(f=>f.FormGroupCode==formGroupCode && !f.IsDeleted)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Güncelleme işlemi sırasında form grubu bulunamadı.");
        bool isFormGroupNameUpdated=(!string.IsNullOrWhiteSpace(dto.FormGroupName)) && (dto.FormGroupName != formGroup.FormGroupName);
        if (isFormGroupNameUpdated)
        {
            bool formGroupExists = await _context.FormGroups
                .AnyAsync(f => (f.FormGroupCode!=formGroup.FormGroupCode) && (f.FormGroupName == dto.FormGroupName));
            if (formGroupExists)
            {
                throw new ConflictException("Form grup ismi zaten kullanılıyor.");
            }
        }
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await UpdateEntityFromDto(formGroup,dto);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        return MapToDto(formGroup);
    }

    public async Task DeleteFormGroupAsync(string formGroupCode)
    {
        FormGroup? formGroup = await _context.FormGroups
                            .Where(f=>f.FormGroupCode==formGroupCode && !f.IsDeleted)
                            .Include(f=>f.Forms)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Silme işlemi sırasında form grubu bulunamadı.");
        formGroup.IsDeleted=true;
        if (formGroup.Forms!=null && formGroup.Forms.Any())
        {
            foreach (var form in formGroup.Forms)
            {
                form.IsDeleted=true;
            }
        }
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.SaveChangesAsync();
            await _menuService.DeleteMenuForFormGroupAsync(formGroup.FormGroupCode);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}