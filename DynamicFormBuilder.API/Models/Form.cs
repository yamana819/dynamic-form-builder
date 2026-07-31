using System;
using System.Collections.Generic;

namespace DynamicFormBuilder.API.Models;

public partial class Form
{
    public Guid FormId { get; set; }

    public string FormName { get; set; } = null!;

    public Guid FormGroupId { get; set; }

    public string? TargetTableName { get; set; }

    public string? TargetPrimaryKey { get; set; }

    public string? ViewName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastUpdate { get; set; }

    public bool? IsDeleted { get; set; }

    public string FormSchema { get; set; } = null!;

    public virtual FormGroup FormGroup { get; set; } = null!;

    public bool IsPublished {get;set;}
}