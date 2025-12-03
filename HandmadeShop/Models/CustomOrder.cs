using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class CustomOrder
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? AdminResponse { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<CustomOrderAttribute> CustomOrderAttributes { get; set; } = new List<CustomOrderAttribute>();

    public virtual User User { get; set; } = null!;
}
