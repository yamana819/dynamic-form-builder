using System.ComponentModel.DataAnnotations;


namespace DynamicFormBuilder.API.DTOs.User;

public class UserChangePasswordDto
{

    [Required(ErrorMessage = "Şifre girmek zorunludur.")]
    [StringLength(255,MinimumLength = 8,ErrorMessage = "Şifre en az 8 en fazla 255 karakterden oluşmalıdır.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf ve bir rakam içermelidir.")]
    public string Password { get; set; } = null!;
    
    [Required(ErrorMessage = "Eski şifreyi girmek zorunludur.")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "Şifre en az 8 en fazla 255 karakterden oluşmalıdır.")]
    public string CurrentPassword {get;set;}=null!;

}