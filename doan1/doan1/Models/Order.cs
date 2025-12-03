using System;
using System.Collections.Generic;

namespace doan1.Models;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal TotalPrice { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? VoucherId { get; set; }

    public string? Description { get; set; }

    public string? ShippingName { get; set; }

    public string? ShippingPhone { get; set; }

    public string? ShippingAddress { get; set; }

    // NEW: Trạng thái thanh toán (nullable để khớp logic lọc: null => chưa thanh toán)
    public bool? IsPaid { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;

    public virtual Voucher? Voucher { get; set; }
}
