using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class CartItem
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime? AddedAt { get; set; }

    public int? VariantId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
