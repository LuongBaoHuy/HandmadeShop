using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class AttributeValue
{
    public int Id { get; set; }

    public int AttributeId { get; set; }

    public int VariantId { get; set; }

    public string Value { get; set; } = null!;

    public virtual Attribute Attribute { get; set; } = null!;

    public virtual ProductVariant Variant { get; set; } = null!;
}
