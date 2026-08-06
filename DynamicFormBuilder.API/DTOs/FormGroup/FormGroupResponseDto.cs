

namespace DynamicFormBuilder.API.DTOs.FormGroup;

public class FormGroupResponseDto
{

    public string FormGroupCode { get; set; }=null!;

    public string FormGroupName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastUpdate { get; set; }

}