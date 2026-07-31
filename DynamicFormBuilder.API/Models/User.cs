using System;
using System.Collections.Generic;

namespace DynamicFormBuilder.API.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = null!;

    public byte RoleId { get; set; }

    public DateTime UserStartDate { get; set; }

    public string PasswordHash { get; set; } = null!;

    public DateTime? UserLastActiveDate { get; set; }    

    public bool IsDeleted { get; set; }

    public virtual Role Role { get; set; } = null!;
}