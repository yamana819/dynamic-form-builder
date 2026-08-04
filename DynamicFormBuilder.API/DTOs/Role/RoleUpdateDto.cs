using DynamicFormBuilder.API.DTOs.Authorization;
using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilder.API.DTOs.Role;

public class RoleUpdateDto
{
    [StringLength(150,MinimumLength = 3,ErrorMessage ="Rol ismi en az 3 en fazla 150 karakter")]
    public string? RoleName { get; set; }

}