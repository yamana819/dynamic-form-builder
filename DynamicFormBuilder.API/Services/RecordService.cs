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

    public async Task<object> GetAllRecordsAsync(Guid formId)
    {
        var viewName = await _context.Forms
                            .Where(f=>f.FormId==formId && !f.IsDeleted && f.IsPublished)
                            .Select(f=>f.ViewName)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form bulunamadı veya henüz yayımlanmamış.");
        var dataTable = await _sqlHelper.GetAllRecordsAsync(viewName);
        
        var columns = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
        var rows = new List<Dictionary<string, object>>();
        
        foreach (DataRow row in dataTable.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in dataTable.Columns)
            {
                dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
            }
            rows.Add(dict);
        }
        
        return new { columns = columns, rows = rows };
    }
    public async Task<Dictionary<string,object>?> GetRecordByIdAsync(Guid formId,string recordId)
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
    public async Task<int> UpdateRecordAsync(Guid formId,string recordId,Dictionary<string,object> formData)
    {
        var form = await _context.Forms
                            .Where(f=>f.FormId==formId && !f.IsDeleted && f.IsPublished)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form bulunamadı veya henüz yayımlanmamış.");
        return await _sqlHelper.UpdateRecordFromJson(form.TargetTableName,form.TargetPrimaryKey,recordId,formData);
    }
    public async Task<int> DeleteRecordAsync(Guid formId, string recordId)
    {
        var form = await _context.Forms
                            .Where(f=>f.FormId==formId && !f.IsDeleted && f.IsPublished)
                            .FirstOrDefaultAsync() ?? throw new ResourceNotFoundException("Form bulunamadı veya henüz yayımlanmamış.");
        return await _sqlHelper.DeleteRecordAsync(form.TargetTableName,form.TargetPrimaryKey,recordId);
    }
}