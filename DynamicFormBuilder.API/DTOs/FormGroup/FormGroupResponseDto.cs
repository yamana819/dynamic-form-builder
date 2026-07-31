

namespace DynamicFormBuilder.API.DTOs.FormGroup;

public class FormGroupResponseDto
{

    public Guid FormGroupId { get; set; }

    public string FormGroupName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastUpdate { get; set; }

}