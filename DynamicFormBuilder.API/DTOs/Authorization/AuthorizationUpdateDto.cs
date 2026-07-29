using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilder.API.DTOs.Authorization;

public class AuthorizationUpdateDto
{
    [Range(1, int.MaxValue,ErrorMessage = "Geçerli bir menü seçilmelidir.")]
    public int? MenuId { get; set; }

    public bool? CanView { get; set; }

    public bool? CanCreate { get; set; }

    public bool? CanEdit { get; set; }

    public bool? CanDelete { get; set; }
}