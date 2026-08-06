using System.ComponentModel.DataAnnotations;


namespace DynamicFormBuilder.API.DTOs.FormGroup;


public class FormGroupUpdateDto
{
    [StringLength(150,MinimumLength = 6,ErrorMessage = "Form grup ismi en az 6 en fazla 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^(?!\s+$)[a-zA-Z0-9\.\-_\sçÇğĞıİöÖşŞüÜ]+$", ErrorMessage = "Form grubu adı sadece harf, rakam, boşluk veya (.,-,_) karakterlerini içerebilir ve sadece boşluktan oluşamaz.")]
    public string? FormGroupName { get; set; }
}