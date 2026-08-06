using System.ComponentModel.DataAnnotations;


namespace DynamicFormBuilder.API.DTOs.Form;


public class FormUpdateDto
{

    [StringLength(150,MinimumLength = 6,ErrorMessage = "Form ismi en az 6 en fazla 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^[a-zA-Z0-9\.\-_çÇğĞıİöÖşŞüÜ]+$", ErrorMessage = "Form  adı sadece harf, rakam veya (.,-,_) karakterlerini içerebilir. Boşluk içeremez.")]
    public string? FormName { get; set; } 

    public string? TargetTableName { get; set; }

    public string? TargetPrimaryKey { get; set; }

    public string? ViewName { get; set; }

    public string? FormSchema { get; set; }

    public string? FormGroupCode { get; set; }

    public bool? IsPublished { get; set; }

}