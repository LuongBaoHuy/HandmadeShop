using System;
using System.Collections.Generic;

namespace doan1.Models;

public partial class ProductAttributeOption
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int AttributeOptionId { get; set; }

    public virtual AttributeOption AttributeOption { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
