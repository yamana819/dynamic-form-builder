
namespace DynamicFormBuilder.API.DTOs.Authorization;


public class AuthorizationResponseDto
{
    public int MenuId { get; set; }

    public string MenuName {get;set;}=null!;

    public bool CanView { get; set; }

    public bool CanCreate { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

}