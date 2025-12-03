using System;
using System.Collections.Generic;

namespace doan1.Models;

public partial class VariationOptionLink
{
    public int Id { get; set; }

    public int VariationId { get; set; }                  // FK -> ProductVariation.Id
    public ProductVariation ProductVariation { get; set; } = default!;

    public int AttributeOptionId { get; set; }            // FK -> AttributeOption.Id
    public AttributeOption AttributeOption { get; set; } = default!;
}
