using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class CustomOrderAttribute
{
    public int Id { get; set; }

    public int CustomOrderId { get; set; }

    public int AttributeId { get; set; }

    public string Value { get; set; } = null!;

    public virtual Attribute Attribute { get; set; } = null!;

    public virtual CustomOrder CustomOrder { get; set; } = null!;
}
