using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilder.API.DTOs.Form;

public class FormCreateDto
{

    [Required(ErrorMessage = "Form adı girilmesi zorunludur.")]
    [StringLength(150,MinimumLength = 6,ErrorMessage = "Form ismi en az 6 en fazla 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^(?!\s+$)[a-zA-Z0-9\.\-_\sçÇğĞıİöÖşŞüÜ]+$", ErrorMessage = "Form adı sadece harf, rakam, boşluk veya (.,-,_) karakterlerini içerebilir ve sadece boşluktan oluşamaz.")]

    public string FormName { get; set; } = null!;

    public string? TargetTableName { get; set; }

    public string? TargetPrimaryKey { get; set; }

    public string? ViewName { get; set; }

    [Required(ErrorMessage = "Form şeması zorunludur.")]
    public string FormSchema { get; set; } = null!;
    [Required(ErrorMessage = "Formun bağlı olduğu grup kodu zorunludur.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Form grup kodu en az 2 en fazla 50 karakterden oluşmalıdır.")]
    [RegularExpression(@"^[A-Z0-9\-_]+$", ErrorMessage = "Form grup kodu sadece büyük harf A-Z, rakam 0-9, - ve _ içerebilir. Boşluk veya Türkçe karakter içeremez.")]
    public string FormGroupCode { get; set; }=null!;
}