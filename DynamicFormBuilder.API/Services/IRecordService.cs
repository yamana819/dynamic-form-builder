using System.Data;

namespace DynamicFormBuilder.API.Services;

public interface IRecordService
{
    Task<object> GetAllRecordsAsync(Guid formId);

    Task<Dictionary<string, object>?> GetRecordByIdAsync(Guid formId, string recordId);

    Task<int> InsertRecordAsync(Guid formId, Dictionary<string, object> formData);

    Task<int> UpdateRecordAsync(Guid formId, string recordId, Dictionary<string, object> formData);

    Task<int> DeleteRecordAsync(Guid formId, string recordId);
}