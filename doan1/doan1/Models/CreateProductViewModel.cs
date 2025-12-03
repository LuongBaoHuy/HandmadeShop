using System.ComponentModel.DataAnnotations;

namespace doan1.Models
{
    public class CreateProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải lớn hơn hoặc bằng 0")]
        public int? Stock { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá gốc phải lớn hơn hoặc bằng 0")]
        public decimal? OriginalPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn hoặc bằng 0")]
        public decimal? DiscountedPrice { get; set; }

        public string? ImageUrl { get; set; }

        // Variation data
        public List<CreateVariationViewModel> Variations { get; set; } = new List<CreateVariationViewModel>();
    }

    public class CreateVariationViewModel
    {
        public string Attributes { get; set; } = string.Empty; // JSON string of attributes
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class VariationAttributeViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
