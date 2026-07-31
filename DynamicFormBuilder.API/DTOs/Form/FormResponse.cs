namespace DynamicFormBuilder.API.DTOs.Form;


public class FormResponseDto
{
     public Guid FormId { get; set; }

    public string FormName { get; set; } = null!;

    public Guid FormGroupId { get; set; }

    public string? TargetTableName { get; set; }

    public string? TargetPrimaryKey { get; set; }

    public string? ViewName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastUpdate { get; set; }

    public string FormSchema { get; set; } = null!;

    public bool IsPublished {get;set;}
}