using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DynamicFormBuilder.API.Models;

public partial class FormGroup
{
    public Guid FormGroupId { get; set; }
    [Required(ErrorMessage = "Form group ismi gereklidir.")]
    [StringLength(150,MinimumLength = 6,ErrorMessage = "Group adı en az 6 en fazla 150 karakterden oluşmalıdır.")]
    [RegularExpression(@"^[a-zA-Z0-9\.\-_çğiöşüÇĞİÖŞÜ]+$",ErrorMessage = "Group adı sadece harf rakam veya (.,-,_) karakterlerini içerebilir.Boşluk içeremez")]
    public string FormGroupName { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? LastUpdate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
}
