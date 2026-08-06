using System;
using System.Collections.Generic;

namespace DynamicFormBuilder.API.Models;

public partial class FormGroup
{
    public string FormGroupCode {get;set;}=null!;

    public string FormGroupName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastUpdate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Form> Forms { get; set; } = new List<Form>();
}