using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilder.API.Models;

public partial class User
{
    public Guid UserId { get; set; }
    [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
    [StringLength(150,MinimumLength = 8,ErrorMessage = "Kullanıcı adı en az 8 en fazla 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^[a-zA-Z0-9\.\-_]+$",ErrorMessage = "Kullanıcı adı sadece harf rakam veya (.,-,_) karakterlerini içerebilir.Boşluk içeremez")]
    public string UserName { get; set; } = null!;
    public byte? RoleId { get; set; }
    public DateTime? UserStartDate { get; set; }
    [Required(ErrorMessage = "Şifre girmek zorunludur.")]
    [StringLength(255,MinimumLength = 8,ErrorMessage = "Şifre en az 8 en fazla 255 karakterden oluşmalıdır.")]
    public string PasswordHash { get; set; } = null!;
    public DateTime? UserLastActiveDate { get; set; }    
    public bool? IsActive { get; set; }
    public virtual Role Role { get; set; } = null!;
}
