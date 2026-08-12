using System.Data;

namespace DynamicFormBuilder.API.Services;

public interface IRecordService
{
    Task<DataTable> GetAllRecordsAsync(Guid formId);

    Task<Dictionary<string, object>?> GetRecordByIdAsync(Guid formId, object recordId);

    Task<int> InsertRecordAsync(Guid formId, Dictionary<string, object> formData);

    Task<int> UpdateRecordAsync(Guid formId, object recordId, Dictionary<string, object> formData);

    Task<int> DeleteRecordAsync(Guid formId, object recordId);
}