using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class AttributeOption
{
    public int Id { get; set; }

    public int AttributeId { get; set; }

    public string Value { get; set; } = null!;

    public virtual Attribute Attribute { get; set; } = null!;

    public virtual ICollection<ProductAttributeOption> ProductAttributeOptions { get; set; } = new List<ProductAttributeOption>();

    public virtual ICollection<VariationOptionLink> VariationOptionLinks { get; set; } = new List<VariationOptionLink>();
}
