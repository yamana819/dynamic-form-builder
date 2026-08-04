using System.ComponentModel.DataAnnotations;
using DynamicFormBuilder.API.DTOs.Authorization;


namespace DynamicFormBuilder.API.DTOs.Role;

public class RoleCreateDto
{
    
    [Required(ErrorMessage = "Rol ismi girmek zorunludur.")]
    [StringLength(150,MinimumLength = 3,ErrorMessage ="Rol ismi en az 3 en fazla 150 karakter")]
    public string RoleName { get; set; } = null!;

}