using System;
using System.Collections.Generic;

namespace DynamicFormBuilder.API.Models;

public partial class Menu
{
    public int MenuId { get; set; }

    public int? ParentMenuId { get; set; }

    public string MenuName { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public string? Href { get; set; }

    public virtual ICollection<Authorization> Authorizations { get; set; } = new List<Authorization>();

    public virtual ICollection<Menu> InverseParentMenu { get; set; } = new List<Menu>();

    public virtual Menu? ParentMenu { get; set; }
}