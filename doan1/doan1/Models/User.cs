using System;
using System.Collections.Generic;

namespace doan1.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Role { get; set; }

    public string? ProfileImageUrl { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<ReviewReply> ReviewReplies { get; set; } = new List<ReviewReply>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
