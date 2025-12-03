namespace doan1.Models
{
    public class ProductAttributeViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
    }

    public class ProductVariationViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string CombinationHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty; // Ví dụ: "Màu Vàng, Kích thước Lớn"
    }

    public class ProductWithVariationsViewModel
    {
        public Product Product { get; set; } = null!;
        public List<ProductAttributeViewModel> Attributes { get; set; } = new List<ProductAttributeViewModel>();
        public List<ProductVariationViewModel> Variations { get; set; } = new List<ProductVariationViewModel>();
        public bool HasVariations => Variations.Any();

        // Thuộc tính để hiển thị giá
        public decimal DisplayPrice => HasVariations ?
            (Variations.Any() ? Variations.Min(v => v.Price) : Product.Price ?? 0) :
            (Product.Price ?? 0);

        public decimal MaxPrice => HasVariations && Variations.Any() ?
            Variations.Max(v => v.Price) : DisplayPrice;

        public bool HasPriceRange => HasVariations && Variations.Any() &&
            Variations.Min(v => v.Price) != Variations.Max(v => v.Price);

        // Tổng số lượng tồn kho
        public int TotalStock => HasVariations ?
            Variations.Sum(v => v.Stock) :
            (Product.Stock ?? 0);
    }

    public class CreateVariationRequest
    {
        public int ProductId { get; set; }
        public List<AttributeOptionCombination> Combinations { get; set; } = new List<AttributeOptionCombination>();
    }

    public class AttributeOptionCombination
    {
        public string AttributeName { get; set; } = string.Empty;
        public string OptionValue { get; set; } = string.Empty;
        public int AttributeOptionId { get; set; }
    }

    public class UpdateVariationRequest
    {
        public int VariationId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
