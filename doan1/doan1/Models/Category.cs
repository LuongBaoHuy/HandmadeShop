using System;
using System.Collections.Generic;

namespace doan1.Models;

public partial class Category
{
    public int Id { get; set; }

    public string CategoriesName { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
