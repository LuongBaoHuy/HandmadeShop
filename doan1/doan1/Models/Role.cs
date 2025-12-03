using System;
using System.Collections.Generic;

namespace doan1.Models;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
