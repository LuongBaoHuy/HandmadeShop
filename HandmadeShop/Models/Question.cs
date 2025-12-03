using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class Question
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public int? ProductId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Product? Product { get; set; }

    public virtual User User { get; set; } = null!;
}
