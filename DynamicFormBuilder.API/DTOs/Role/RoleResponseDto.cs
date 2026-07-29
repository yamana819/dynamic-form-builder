using DynamicFormBuilder.API.DTOs.Authorization;

namespace DynamicFormBuilder.API.DTOs.Role;


public class RoleResponseDto
{
    public byte RoleId { get; set; }

    public string RoleName { get; set; } = null!;

   public List<AuthorizationResponseDto>? Authorizations {get;set;}
}