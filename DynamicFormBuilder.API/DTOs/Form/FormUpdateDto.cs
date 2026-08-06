using System.ComponentModel.DataAnnotations;


namespace DynamicFormBuilder.API.DTOs.Form;


public class FormUpdateDto
{

    [StringLength(150,MinimumLength = 6,ErrorMessage = "Form ismi en az 6 en fazla 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^(?!\s+$)[a-zA-Z0-9\.\-_\sçÇğĞıİöÖşŞüÜ]+$", ErrorMessage = "Form adı sadece harf, rakam, boşluk veya (.,-,_) karakterlerini içerebilir ve sadece boşluktan oluşamaz.")]

    public string? FormName { get; set; } 

    public string? TargetTableName { get; set; }

    public string? TargetPrimaryKey { get; set; }

    public string? ViewName { get; set; }

    public string? FormSchema { get; set; }

    public bool? IsPublished { get; set; }

}