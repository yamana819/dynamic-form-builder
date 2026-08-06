using DynamicFormBuilder.API.DTOs.Authorization;
using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilder.API.DTOs.Role;

public class RoleUpdateDto
{
    [StringLength(150,MinimumLength = 3,ErrorMessage ="Rol ismi en az 3 en fazla 150 karakter")]
    [RegularExpression(@"^(?!\s+$)[a-zA-Z0-9\.\-_\sçÇğĞıİöÖşŞüÜ]+$", ErrorMessage = "Rol ismi sadece harf, rakam, boşluk veya (.,-,_) karakterlerini içerebilir ve sadece boşluktan oluşamaz.")]
    public string? RoleName { get; set; }

}