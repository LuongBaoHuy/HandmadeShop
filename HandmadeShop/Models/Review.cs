using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class Review
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? VariantId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ReviewReply> ReviewReplies { get; set; } = new List<ReviewReply>();

    public virtual User User { get; set; } = null!;
}
