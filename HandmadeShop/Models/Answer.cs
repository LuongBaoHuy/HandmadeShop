using System;
using System.Collections.Generic;

namespace HandmadeShop.Models;

public partial class Answer
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public int? ParentAnswerId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Answer> InverseParentAnswer { get; set; } = new List<Answer>();

    public virtual Answer? ParentAnswer { get; set; }

    public virtual Question Question { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
