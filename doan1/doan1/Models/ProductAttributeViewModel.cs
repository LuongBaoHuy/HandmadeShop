using System.Collections.Generic;

namespace doan1.Models
{
    public class ProductAttributeDetailViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public List<AttributeWithOptionsDetail> Attributes { get; set; } = new List<AttributeWithOptionsDetail>();
    }

    public class AttributeWithOptionsDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<AttributeOptionDetailDto> Options { get; set; } = new List<AttributeOptionDetailDto>();
    }

    public class AttributeOptionDetailDto
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
}
