using System;
using System.Collections.Generic;

namespace doan1.Models;

public partial class AttributeOption
{
    public int Id { get; set; }

    public int AttributeId { get; set; }

    public string Value { get; set; } = null!;

    public virtual Attribute Attribute { get; set; } = null!;

    public virtual ICollection<ProductAttributeOption> ProductAttributeOptions { get; set; } = new List<ProductAttributeOption>();

    public ICollection<VariationOptionLink> VariationOptionLinks { get; set; } = new List<VariationOptionLink>();
}
