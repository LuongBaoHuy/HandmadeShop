using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace doan1.Models
{
    [Table("ProductImages")]
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsMain { get; set; }
    }
}
