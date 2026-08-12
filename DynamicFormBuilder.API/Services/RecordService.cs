using System.Data;
using DynamicFormBuilder.API.Data;
using DynamicFormBuilder.API.Data.Helpers;
using DynamicFormBuilder.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DynamicFormBuilder.API.Services;


public class RecordService : IRecordService
{
    private readonly SqlHelper _sqlHelper;
    private readonly DynamicFormBuilderDbContext _context;
    public RecordService(DynamicFormBuilderDbContext context,SqlHelper sqlHelper)
    {
        _context=context;
        _sqlHelper=sqlHelper;
    }

    public async Task<DataTable> GetAllRecordsAsync(Guid formId)
    {
        var viewName = await _context.Forms
                            .Where(f=>f.FormId==formId && !f.IsDeleted && f.IsPublished)
                            .Select(f=>f.ViewName)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form bulunamadı veya henüz yayımlanmamış.");
        var data = await _sqlHelper.GetAllRecordsAsync(viewName);
        return data;
    }
    public async Task<Dictionary<string,object>?> GetRecordByIdAsync(Guid formId,object recordId)
    {
        var form = await _context.Forms
                            .Where(f=>f.FormId==formId && !f.IsDeleted && f.IsPublished)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form bulunamadı veya henüz yayımlanmamış.");
        return await _sqlHelper.GetRecordByIdAsync(form.TargetTableName,form.TargetPrimaryKey,recordId);
    }
    public async Task<int> InsertRecordAsync(Guid formId,Dictionary<string,object> formData)
    {
        var tableName = await _context.Forms
                            .Where(f=>f.FormId==formId && !f.IsDeleted && f.IsPublished)
                            .Select(f=>f.TargetTableName)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form bulunamadı veya henüz yayımlanmamış.");
        return await _sqlHelper.InsertRecordFromJson(tableName,formData); 
    }
    public async Task<int> UpdateRecordAsync(Guid formId,object recordId,Dictionary<string,object> formData)
    {
        var form = await _context.Forms
                            .Where(f=>f.FormId==formId && !f.IsDeleted && f.IsPublished)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form bulunamadı veya henüz yayımlanmamış.");
        return await _sqlHelper.UpdateRecordFromJson(form.TargetTableName,form.TargetPrimaryKey,recordId,formData);
    }
    public async Task<int> DeleteRecordAsync(Guid formId, object recordId)
    {
        var form = await _context.Forms
                            .Where(f=>f.FormId==formId && !f.IsDeleted && f.IsPublished)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form bulunamadı veya henüz yayımlanmamış.");
        return await _sqlHelper.DeleteRecordAsync(form.TargetTableName,form.TargetPrimaryKey,recordId);
    }
}