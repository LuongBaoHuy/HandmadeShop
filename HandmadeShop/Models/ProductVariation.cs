using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class ProductVariation
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string? CombinationHash { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<VariationOptionLink> VariationOptionLinks { get; set; } = new List<VariationOptionLink>();
}
