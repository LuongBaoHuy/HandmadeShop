using System;
using System.Collections.Generic;

namespace doan1.Models;

public partial class ReviewReply
{
    public int Id { get; set; }

    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public string? Reply { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Review Review { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
