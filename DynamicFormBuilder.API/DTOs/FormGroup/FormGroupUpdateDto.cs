using System.ComponentModel.DataAnnotations;


namespace DynamicFormBuilder.API.DTOs.FormGroup;


public class FormGroupUpdateDto
{
    [StringLength(150,MinimumLength = 6,ErrorMessage = "Form grup ismi en az 6 en fazla 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^[a-zA-Z0-9\.\-_çÇğĞıİöÖşŞüÜ]+$", ErrorMessage = "Form grubu adı sadece harf, rakam veya (.,-,_) karakterlerini içerebilir. Boşluk içeremez.")]

    public string? FormGroupName { get; set; } 

}