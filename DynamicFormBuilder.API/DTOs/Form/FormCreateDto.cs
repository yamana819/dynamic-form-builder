using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilder.API.DTOs.Form;

public class FormCreateDto
{

    [Required(ErrorMessage = "Form adı girilmesi zorunludur.")]
    [StringLength(150,MinimumLength = 6,ErrorMessage = "Form ismi en az 6 en fazla 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^[a-zA-Z0-9\.\-_çÇğĞıİöÖşŞüÜ]+$", ErrorMessage = "Form  adı sadece harf, rakam veya (.,-,_) karakterlerini içerebilir. Boşluk içeremez.")]
    public string FormName { get; set; } = null!;

    public string? TargetTableName { get; set; }

    public string? TargetPrimaryKey { get; set; }

    public string? ViewName { get; set; }

    [Required(ErrorMessage = "Form şeması zorunludur.")]
    public string FormSchema { get; set; } = null!;

    public Guid FormGroupId { get; set; }

}