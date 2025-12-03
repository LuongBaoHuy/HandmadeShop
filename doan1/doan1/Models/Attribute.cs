using System;
using System.Collections.Generic;

namespace doan1.Models;

public partial class Attribute
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<AttributeOption> AttributeOptions { get; set; } = new List<AttributeOption>();
}
