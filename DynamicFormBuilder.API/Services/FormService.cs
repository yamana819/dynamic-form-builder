using DynamicFormBuilder.API.Models;
using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.DTOs.Form;
using Microsoft.EntityFrameworkCore;
using DynamicFormBuilder.API.Exceptions;
using System.Text.RegularExpressions;

namespace DynamicFormBuilder.API.Services;


public class FormService : IFormService
{
    private readonly DynamicFormBuilderDbContext _context;
    
    private readonly IMenuService _menuService;

    public FormService(DynamicFormBuilderDbContext context,IMenuService menuService)
    {
        _context=context;
        _menuService=menuService;
    }
    private Form MapToForm(FormCreateDto dto)
    {
        return new Form
        {
            FormName=Regex.Replace(dto.FormName,@"\s+"," ").Trim(),
            TargetTableName=dto.TargetTableName,
            TargetPrimaryKey=dto.TargetPrimaryKey,
            ViewName=dto.ViewName,
            FormSchema=dto.FormSchema,
            FormGroupCode=dto.FormGroupCode
        };
    }
    private FormResponseDto MapToDto(Form form)
    {
        return new FormResponseDto
        {
            FormId=form.FormId,
            FormName=form.FormName,
            FormGroupCode=form.FormGroupCode,
            TargetTableName=form.TargetTableName,
            TargetPrimaryKey=form.TargetPrimaryKey,
            ViewName=form.ViewName,
            CreatedAt=form.CreatedAt,
            LastUpdate=form.LastUpdate,
            FormSchema=form.FormSchema,
            IsPublished=form.IsPublished
        };
    }

    private async Task UpdateEntityFromDto(Form form,FormUpdateDto dto)
    {
        bool isUpdated = false;
        if (form.IsPublished)
        {
            bool isTableChanged = !string.IsNullOrWhiteSpace(dto.TargetTableName) && (dto.TargetTableName!=form.TargetTableName);
            bool isPrimaryKeyChanged=!string.IsNullOrWhiteSpace(dto.TargetPrimaryKey) && (dto.TargetPrimaryKey!=form.TargetPrimaryKey);
            bool isSchemaChanged = !string.IsNullOrWhiteSpace(dto.FormSchema) && (dto.FormSchema!=form.FormSchema);
            if (isTableChanged || isPrimaryKeyChanged || isSchemaChanged)
            {
                throw new BadRequestException("Form yayımlandıktan sonra form şeması,tablo adı ve tablo primary key ismi değiştirilemez.");
            }
        }
        if (!string.IsNullOrWhiteSpace(dto.FormName) && (form.FormName!=dto.FormName))
        {
            form.FormName=Regex.Replace(dto.FormName,@"\s+"," ").Trim();
            await _menuService.UpdateMenuForFormAsync(form.FormId,form.FormGroupCode,form.FormName);
            isUpdated=true;
        }
        if (!string.IsNullOrWhiteSpace(dto.TargetTableName) && (form.TargetTableName!=dto.TargetTableName))
        {
            form.TargetTableName=dto.TargetTableName;
            isUpdated=true;
        }
        if (!string.IsNullOrWhiteSpace(dto.TargetPrimaryKey) && (form.TargetPrimaryKey!=dto.TargetPrimaryKey))
        {
            form.TargetPrimaryKey=dto.TargetPrimaryKey;
            isUpdated=true;
        }
        if (!string.IsNullOrWhiteSpace(dto.ViewName) && (form.ViewName!=dto.ViewName))
        {
            form.ViewName=dto.ViewName;
            isUpdated=true;
        }
        if (!string.IsNullOrWhiteSpace(dto.FormSchema) && (form.FormSchema!=dto.FormSchema))
        {
            form.FormSchema=dto.FormSchema;
            isUpdated=true;
        }
        if (isUpdated)
        {
            form.LastUpdate=DateTime.Now;
        }
    }
    public async Task<IEnumerable<FormResponseDto>> GetFormsByGroupAsync(string formGroupCode,int pageNumber=1,int pageSize=50)
    {
        return await _context.Forms
            .Where(f=>f.FormGroupCode==formGroupCode && !f.IsDeleted)
            .OrderBy(f=>f.FormId)
            .Skip((pageNumber-1)*pageSize)
            .Take(pageSize)
            .Select(f=>new FormResponseDto {
                FormId=f.FormId,
                FormName=f.FormName,
                FormGroupCode=f.FormGroupCode,
                TargetTableName=f.TargetTableName,
                TargetPrimaryKey=f.TargetPrimaryKey,
                ViewName=f.ViewName,
                CreatedAt=f.CreatedAt,
                LastUpdate=f.LastUpdate,
                FormSchema=f.FormSchema,
                IsPublished=f.IsPublished
            })
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<FormResponseDto> GetFormAsync(Guid formId)
    {
        Form? form = await _context.Forms
                    .Where(f=>f.FormId==formId && !f.IsDeleted)
                    .AsNoTracking()
                    .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form bulunamadı.");
        return MapToDto(form);
    }
    public async Task<FormResponseDto> CreateFormAsync(FormCreateDto dto,byte creatorRoleId)
    {
        bool formGroupExists = await _context.FormGroups.AnyAsync(f=>f.FormGroupCode==dto.FormGroupCode);
        bool formExists = await _context.Forms.AnyAsync(f=>f.FormName==dto.FormName);
        if (!formGroupExists)
        {
            throw new ResourceNotFoundException("Belirtilen form grubu bulunamadı");
        }
        if (formExists)
        {
            throw new ConflictException("Bu form ismi zaten kullanılıyor.");
        }
        Form form = MapToForm(dto);
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Forms.Add(form);
            await _context.SaveChangesAsync();
            await _menuService.CreateMenuForFormAsync(form.FormId,form.FormGroupCode,form.FormName,creatorRoleId);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        return MapToDto(form);
    }

    public async Task<FormResponseDto> UpdateFormAsync(Guid formId,FormUpdateDto dto)
    {
        Form? form = await _context.Forms
                        .Where(f=>f.FormId==formId && !f.IsDeleted)
                        .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Güncelleme işlemi sırasında form bulunamadı.");
        if (!string.IsNullOrWhiteSpace(dto.FormName) && (dto.FormName != form.FormName))
        {
            bool formExists = await _context.Forms
                .AnyAsync(f => f.FormName == dto.FormName);
            if (formExists)
            {
                throw new ConflictException($"'{dto.FormName}' form ismi zaten kullanılıyor.");
            }
        }
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await UpdateEntityFromDto(form,dto);
            await _context.SaveChangesAsync();    
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        return MapToDto(form);
    }
    public async Task DeleteFormAsync(Guid formId)
    {
        Form? form = await _context.Forms
                        .Where(f=>f.FormId==formId && !f.IsDeleted)
                        .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Silme işlemi sırasında form bulunamadı.");
        form.IsDeleted=true;
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.SaveChangesAsync();
            await _menuService.DeleteMenuForFormAsync(form.FormId,form.FormGroupCode);
            await transaction.CommitAsync();
        }catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<FormResponseDto> PublishFormAsync(Guid formId)
    {
        Form? form = await _context.Forms
                        .Where(f=>f.FormId==formId && !f.IsDeleted)
                        .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Yayınlama işlemi sırasında form bulunamadı.");
        if (string.IsNullOrWhiteSpace(form.TargetTableName) || string.IsNullOrWhiteSpace(form.TargetPrimaryKey) || string.IsNullOrWhiteSpace(form.ViewName))
        {
            throw new BadRequestException("Formu yayımlamadan önce verilerin kaydedileceği tablo view ismi ve primary key ismi girmek zorunludur");
        }
        form.IsPublished=true;
        form.LastUpdate=DateTime.Now;
        await _context.SaveChangesAsync();
        return MapToDto(form);
    }
}