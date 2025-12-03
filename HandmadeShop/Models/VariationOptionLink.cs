using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class VariationOptionLink
{
    public int Id { get; set; }

    public int VariationId { get; set; }

    public int AttributeOptionId { get; set; }

    public virtual AttributeOption AttributeOption { get; set; } = null!;

    public virtual ProductVariation Variation { get; set; } = null!;
}
