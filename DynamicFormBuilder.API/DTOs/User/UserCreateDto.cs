using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilder.API.DTOs.User;

public class UserCreateDto
{
    [Required(ErrorMessage ="Kullanıcı adı girmek gereklidir.")]
    [StringLength(150,MinimumLength = 6,ErrorMessage = "Kullanıcı adı minimum 6 maksimum 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^[a-zA-Z0-9\.\-_]+$",ErrorMessage = "Kullanıcı adı sadece harf rakam veya (.,-,_) karakterlerini içerebilir.Boşluk içeremez")]
    public string UserName { get; set; } = null!;
    [Required(ErrorMessage = "Şifre girmek zorunludur.")]
    [StringLength(255,MinimumLength = 8,ErrorMessage = "Şifre en az 8 en fazla 255 karakterden oluşmalıdır.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf ve bir rakam içermelidir.")]
    public string Password { get; set; } = null!;
}