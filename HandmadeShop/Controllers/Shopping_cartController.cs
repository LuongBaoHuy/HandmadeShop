using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HandmadeShop.Controllers
{
    public class Shopping_cartController : BaseController
    {
        private readonly ILogger<Shopping_cartController> _logger;

        public Shopping_cartController(HandmadeShopContext context, ILogger<Shopping_cartController> logger) : base(context)
        {
            _logger = logger;
        }

        // Hiển thị giỏ hàng
        public IActionResult Shopping_cart()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cartItems = db.CartItems
                .Where(ci => ci.UserId == user.Id)
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.ProductVariations)
                        .ThenInclude(v => v.VariationOptionLinks)
                            .ThenInclude(vol => vol.AttributeOption)
                                .ThenInclude(ao => ao.Attribute)
                .Include(ci => ci.Product.ProductAttributeOptions)
                    .ThenInclude(pao => pao.AttributeOption)
                        .ThenInclude(ao => ao.Attribute)
                .ToList();

            ViewBag.CartItems = cartItems;
            ViewBag.Total = cartItems.Sum(ci =>
            {
                var variation = ci.VariantId.HasValue
                    ? ci.Product.ProductVariations.FirstOrDefault(v => v.Id == ci.VariantId.Value)
                    : null;
                var discountRate = ci.Product.DiscountedPrice ?? 0;
                decimal price = 0;
                if (variation != null)
                    price = (discountRate > 0) ? variation.Price * (1 - discountRate) : variation.Price;
                else
                    price = (discountRate > 0) ? (ci.Product.Price ?? 0) * (1 - discountRate) : (ci.Product.Price ?? 0);
                return price * ci.Quantity;
            });
            return View();
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1, int? variationId = null)
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!", requireLogin = true });

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng!" });

            var product = db.Products
                .Include(p => p.ProductVariations)
                .FirstOrDefault(p => p.Id == productId);

            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

            if (quantity <= 0)
                return Json(new { success = false, message = "Số lượng không hợp lệ!" });

            int availableStock = product.Stock ?? 0;
            decimal price = product.Price ?? 0;
            ProductVariation? selectedVariation = null;

            if (variationId.HasValue)
            {
                selectedVariation = product.ProductVariations.FirstOrDefault(v => v.Id == variationId.Value);
                if (selectedVariation == null)
                    return Json(new { success = false, message = "Phân loại sản phẩm không hợp lệ!" });
                availableStock = selectedVariation.Stock;
                price = selectedVariation.Price;
            }

            if (availableStock < quantity)
                return Json(new { success = false, message = $"Chỉ còn {availableStock} sản phẩm trong kho!" });

            var cartItem = db.CartItems.FirstOrDefault(ci =>
                ci.UserId == user.Id &&
                ci.ProductId == productId &&
                ci.VariantId == variationId);

            if (cartItem != null)
            {
                int newQuantity = cartItem.Quantity + quantity;
                if (newQuantity > availableStock)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Số lượng vượt quá tồn kho! Hiện tại bạn đã có {cartItem.Quantity} sản phẩm trong giỏ hàng."
                    });
                }
                cartItem.Quantity = newQuantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = user.Id,
                    ProductId = productId,
                    VariantId = variationId,
                    Quantity = quantity,
                    AddedAt = DateTime.Now
                };
                db.CartItems.Add(cartItem);
            }

            db.SaveChanges();

            var cartCount = db.CartItems.Where(ci => ci.UserId == user.Id).Sum(ci => ci.Quantity);
            var totalPrice = db.CartItems
                .Where(ci => ci.UserId == user.Id)
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.ProductVariations)
                .ToList()
                .Sum(ci =>
                {
                    var v = ci.VariantId.HasValue
                        ? ci.Product.ProductVariations.FirstOrDefault(va => va.Id == ci.VariantId.Value)
                        : null;
                    var p = v != null ? v.Price : (ci.Product.DiscountedPrice ?? ci.Product.Price ?? 0);
                    return p * ci.Quantity;
                });

            return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng thành công!", cartCount = cartCount, totalPrice = totalPrice });
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public IActionResult RemoveFromCart(int productId, int? variationId = null)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cartItem = db.CartItems.FirstOrDefault(ci => ci.UserId == user.Id && ci.ProductId == productId );
            if (cartItem != null)
            {
                db.CartItems.Remove(cartItem);
                db.SaveChanges();
            }
            return RedirectToAction("Shopping_cart");
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity, int? variationId = null)
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false, message = "Vui lòng đăng nhập để cập nhật giỏ hàng!" });

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng!" });

            var cartItem = db.CartItems.FirstOrDefault(ci => ci.UserId == user.Id && ci.ProductId == productId && ci.VariantId == variationId);

            int? maxStock = null;
            if (variationId.HasValue)
            {
                var product = db.Products.Include(p => p.ProductVariations).FirstOrDefault(p => p.Id == productId);
                var variation = product?.ProductVariations.FirstOrDefault(v => v.Id == variationId.Value);
                maxStock = variation?.Stock;
            }
            else
            {
                var product = db.Products.Find(productId);
                maxStock = product?.Stock;
            }

            if (cartItem != null && quantity > 0 && quantity <= (maxStock ?? 0))
            {
                cartItem.Quantity = quantity;
                db.SaveChanges();
            }
            return RedirectToAction("Shopping_cart");
        }

        [HttpPost]
        public IActionResult ApplyVoucher(string voucherCode)
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false, message = "Vui lòng đăng nhập để áp dụng voucher!" });

            var voucher = db.Vouchers.FirstOrDefault(v =>
                v.Code == voucherCode &&
                v.IsActive == true &&
                (v.ExpiryDate == null || v.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now)));

            if (voucher == null)
                return Json(new { success = false, message = "Voucher không hợp lệ hoặc đã hết hạn!" });

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng!" });

            var cartItems = db.CartItems
                .Where(ci => ci.UserId == user.Id)
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.ProductVariations)
                .ToList();

            var total = cartItems.Sum(ci =>
            {
                var variation = ci.VariantId.HasValue
                    ? ci.Product.ProductVariations.FirstOrDefault(v => v.Id == ci.VariantId.Value)
                    : null;
                var discountRate = ci.Product.DiscountedPrice ?? 0;
                decimal price = 0;
                if (variation != null)
                    price = (discountRate > 0) ? variation.Price * (1 - discountRate) : variation.Price;
                else
                    price = (discountRate > 0) ? (ci.Product.Price ?? 0) * (1 - discountRate) : (ci.Product.Price ?? 0);
                return price * ci.Quantity;
            });

            if (voucher.MinOrderValue.HasValue && total < voucher.MinOrderValue.Value)
                return Json(new { success = false, message = "Tổng giá trị đơn hàng không đủ để áp dụng voucher!" });

            decimal discount = voucher.DiscountType.ToLower() == "percent"
                ? total * (voucher.DiscountValue / 100)
                : voucher.DiscountValue;

            discount = Math.Min(discount, total); // Không vượt quá tổng tiền
            var discountedTotal = total - discount;

            HttpContext.Session.SetString("VoucherCode", voucherCode);
            HttpContext.Session.SetInt32("DiscountedTotal", (int)discountedTotal);

            return Json(new { success = true, message = "Voucher áp dụng thành công!", discountedTotal });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
