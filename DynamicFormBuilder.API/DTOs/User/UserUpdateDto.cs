using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilder.API.DTOs.User;

public class UserUpdateDto
{
    
    [StringLength(150,MinimumLength = 4,ErrorMessage = "Kullanıcı adı minimum 4 maksimum 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^[a-zA-Z0-9\.\-_]+$",ErrorMessage = "Kullanıcı adı sadece harf rakam veya (.,-,_) karakterlerini içerebilir.Boşluk içeremez")]
    public string? UserName { get; set; }

}