using System.ComponentModel.DataAnnotations;


namespace DynamicFormBuilder.API.DTOs.FormGroup;


public class FormGroupCreateDto
{
    [Required(ErrorMessage = "Form grubu adı girilmesi zorunludur.")]
    [StringLength(150,MinimumLength = 6,ErrorMessage = "Form grup ismi en az 6 en fazla 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^(?!\s+$)[a-zA-Z0-9\.\-_\sçÇğĞıİöÖşŞüÜ]+$", ErrorMessage = "Form grubu adı sadece harf, rakam, boşluk veya (.,-,_) karakterlerini içerebilir ve sadece boşluktan oluşamaz.")]
    public string FormGroupName { get; set; } = null!;

    [Required(ErrorMessage = "Form grubu oluştururken group kodu girmek zorunludur.")]
    [StringLength(50,MinimumLength = 2,ErrorMessage = "Form grup kodu en fazla 50 karakterden oluşmalıdır.")]
    [RegularExpression(@"^[A-Z0-9\-_]+$", ErrorMessage = "Form grup kodu sadece büyük harf A-Z ve rakam 0-9 -,_ içerebilir herhangi bir türkçe karakter içeremez ")]
    public string FormGroupCode {get;set;}=null!;

}