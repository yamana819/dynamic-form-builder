

namespace DynamicFormBuilder.API.Services;


  public interface ISchemaService
    {
        Task<bool> ValidatePublishRequirementsAsync(string tableName, string primaryKey, string? viewName, string formSchema);
        Task ValidateTableAndPrimaryKeyAsync(string tableName, string primaryKey);
        Task ValidateViewExistsAsync(string viewName);
        Task ValidateSchemaCompatibilityAsync(string tableName, string formSchema);
    }
