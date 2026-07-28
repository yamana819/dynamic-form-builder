using System;
using System.Collections.Generic;

namespace DynamicFormBuilder.API.Models;

public partial class Authorization
{
    public byte RoleId { get; set; }

    public int MenuId { get; set; }

    public bool CanView { get; set; }

    public bool CanCreate { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public virtual Menu Menu { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
