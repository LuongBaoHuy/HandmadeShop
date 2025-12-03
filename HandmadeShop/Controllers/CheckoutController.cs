using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HandmadeShop.Controllers
{
    public class CheckoutController : BaseController
    {
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(HandmadeShopContext context, ILogger<CheckoutController> logger) : base(context)
        {
            _logger = logger;
        }

        // Hiển thị trang checkout
        [HttpGet]
        public IActionResult Checkout()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Login", "Account"); // Đổi từ "User" thành "Account"

                var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (user == null)
                    return RedirectToAction("Login", "Account");

                // Lấy giỏ hàng với error handling
                var cartItems = db.CartItems
                    .Where(ci => ci.UserId == user.Id)
                    .Include(ci => ci.Product)
                    .ThenInclude(p => p.ProductVariations)
                    .ToList() ?? new List<CartItem>();

                ViewBag.CartItems = cartItems;
                ViewBag.FullName = user.FullName ?? "";
                ViewBag.Phone = user.Phone ?? "";
                ViewBag.Address = user.Address ?? "";

                // Tính tổng tiền với error handling
                ViewBag.Total = cartItems.Sum(ci =>
                {
                    decimal price = 0;
                    decimal discountRate = ci.Product?.DiscountedPrice ?? 0;
                    if (ci.VariantId != null && ci.Product?.ProductVariations != null)
                    {
                        var variation = ci.Product.ProductVariations.FirstOrDefault(v => v.Id == ci.VariantId.Value);
                        if (variation != null)
                            price = (discountRate > 0) ? variation.Price * (1 - discountRate) : variation.Price;
                        else
                            price = (discountRate > 0) ? (ci.Product.Price ?? 0) * (1 - discountRate) : (ci.Product.Price ?? 0);
                    }
                    else
                    {
                        price = (discountRate > 0) ? (ci.Product.Price ?? 0) * (1 - discountRate) : (ci.Product.Price ?? 0);
                    }
                    return price * ci.Quantity;
                });

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Checkout GET action");
                return View("Error"); // Hoặc redirect về trang chủ
            }
        }

        // Xử lý đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(
            string FullName,
            string Phone,
            string Address,
            string VoucherCode,
            string Note,
            string PaymentMethod,
            bool isBuyNow = false,
            int? productId = null,
            int quantity = 1,
            int? variationId = null)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Cho phép trường Note để trống (tránh lỗi [Required] ở model Order.Note)
            Note = (Note ?? string.Empty).Trim();
            ModelState.Remove(nameof(Note));
            ModelState.Remove("Order.Note"); // trường hợp client gửi lên là Order.Note

            // Cho phép VoucherCode để trống (tránh lỗi [Required] ở model Order.VoucherCode)
            VoucherCode = (VoucherCode ?? string.Empty).Trim();
            ModelState.Remove(nameof(VoucherCode));
            ModelState.Remove("Order.VoucherCode"); // trường hợp client gửi lên là Order.VoucherCode

            // Xử lý luồng MUA NGAY
            if (isBuyNow && productId.HasValue)
            {
                var product = db.Products
                    .Include(p => p.ProductVariations)
                    .FirstOrDefault(p => p.Id == productId.Value);

                if (product == null)
                {
                    ModelState.AddModelError("", "Sản phẩm không tồn tại.");
                    return RedirectToAction("Detail", "Detail", new { id = productId.Value });
                }

                // Unit price (apply product discount % if any)
                decimal discountRate = product.DiscountedPrice ?? 0m;
                int availableStock;
                decimal unitPrice;

                if (variationId.HasValue)
                {
                    var v = product.ProductVariations.FirstOrDefault(x => x.Id == variationId.Value);
                    if (v == null)
                    {
                        ModelState.AddModelError("", "Phân loại sản phẩm không hợp lệ.");
                        return RedirectToAction("Detail", "Detail", new { id = product.Id });
                    }
                    availableStock = v.Stock;
                    unitPrice = (discountRate > 0) ? v.Price * (1 - discountRate) : v.Price;
                }
                else
                {
                    availableStock = product.Stock ?? 0;
                    var basePrice = product.Price ?? 0m;
                    unitPrice = (discountRate > 0) ? basePrice * (1 - discountRate) : basePrice;
                }

                if (quantity <= 0)
                {
                    ModelState.AddModelError("", "Số lượng không hợp lệ.");
                }
                if (availableStock < quantity)
                {
                    ModelState.AddModelError("", $"Chỉ còn {availableStock} sản phẩm trong kho.");
                }

                var cartItemView = new CartItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = quantity,
                    VariantId = variationId
                };

                // Tính tổng tiền và áp dụng voucher nếu có
                var totalPrice = unitPrice * quantity;

                decimal discount = 0m;
                int? voucherId = null;
                if (!string.IsNullOrWhiteSpace(VoucherCode))
                {
                    var voucher = db.Vouchers.FirstOrDefault(v =>
                        v.Code == VoucherCode && v.IsActive == true &&
                        (v.ExpiryDate == null || v.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now)));

                    if (voucher == null)
                    {
                        ModelState.AddModelError("", "Mã voucher không hợp lệ hoặc đã hết hạn.");
                    }
                    else
                    {
                        if (voucher.MinOrderValue.HasValue && totalPrice < voucher.MinOrderValue.Value)
                        {
                            ModelState.AddModelError("", $"Tổng giá trị đơn hàng phải từ {voucher.MinOrderValue.Value:N0}₫ để áp dụng voucher này.");
                        }
                        else
                        {
                            discount = voucher.DiscountType.ToLower() == "percent"
                                ? totalPrice * voucher.DiscountValue
                                : voucher.DiscountValue;
                            discount = Math.Min(discount, totalPrice);
                            voucherId = voucher.Id;

                            ViewBag.VoucherType = voucher.DiscountType.ToLower();
                            ViewBag.VoucherValue = voucher.DiscountValue;
                        }
                    }
                }

                if (!ModelState.IsValid)
                {
                    // Refill view for errors
                    ViewBag.IsBuyNow = true;
                    ViewBag.BuyNowProductId = product.Id;
                    ViewBag.BuyNowQuantity = quantity;
                    ViewBag.BuyNowVariationId = variationId;

                    ViewBag.CartItems = new List<CartItem> { cartItemView };
                    ViewBag.FullName = user.FullName ?? "";
                    ViewBag.Phone = user.Phone ?? "";
                    ViewBag.Address = user.Address ?? "";
                    ViewBag.Total = totalPrice;
                    ViewBag.Discount = discount;
                    ViewBag.TotalAfterDiscount = totalPrice - discount;

                    return View("Checkout");
                }

                // Create order for buy-now item only
                var order = new Order
                {
                    UserId = user.Id,
                    TotalPrice = totalPrice - discount,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    VoucherId = voucherId,
                    Description = Note,
                    ShippingName = FullName,
                    ShippingPhone = Phone,
                    ShippingAddress = Address,
                    IsPaid = false // mặc định
                };
                db.Orders.Add(order);
                db.SaveChanges();

                db.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    VariantId = variationId,
                    Quantity = quantity,
                    Price = unitPrice
                });

                db.SaveChanges();

                // Sau khi tạo order (BUY-NOW), điều hướng theo phương thức thanh toán
                if (!string.IsNullOrEmpty(PaymentMethod) && PaymentMethod.Equals("Bank", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Momo", "Payment", new { id = order.Id });
                }

                TempData["OrderSuccess"] = "Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.";
                return RedirectToAction("OrderDetail", new { id = order.Id });
            }

            // CART FLOW (existing) -------------------------
            // Lấy lại selectedItems từ TempData
            int[] selectedItems = Array.Empty<int>();
            if (TempData["SelectedCartItems"] != null)
            {
                selectedItems = TempData["SelectedCartItems"].ToString().Split(',').Select(int.Parse).ToArray();
            }

            IQueryable<CartItem> cartQuery = db.CartItems
                 .Where(ci => ci.UserId == user.Id)
                 .Include(ci => ci.Product)
                 .ThenInclude(p => p.ProductVariations);

            if (selectedItems.Length > 0)
                cartQuery = cartQuery.Where(ci => selectedItems.Contains(ci.Id));

            var cartItems = cartQuery.ToList();

            if (!cartItems.Any())
            {
                ModelState.AddModelError("", "Giỏ hàng của bạn đang trống.");

                ViewBag.CartItems = cartItems;
                ViewBag.FullName = user.FullName ?? "";
                ViewBag.Phone = user.Phone ?? "";
                ViewBag.Address = user.Address ?? "";
                ViewBag.Total = 0m;
                ViewBag.Discount = 0m;
                ViewBag.TotalAfterDiscount = 0m;

                return View("Checkout");
            }

            decimal UnitPrice(CartItem ci)
            {
                decimal discountRate = ci.Product?.DiscountedPrice ?? 0m;
                if (ci.VariantId != null && ci.Product?.ProductVariations != null)
                {
                    var v = ci.Product.ProductVariations.FirstOrDefault(x => x.Id == ci.VariantId.Value);
                    if (v != null) return (discountRate > 0) ? v.Price * (1 - discountRate) : v.Price;
                }
                var basePrice = ci.Product?.Price ?? 0m;
                return (discountRate > 0) ? basePrice * (1 - discountRate) : basePrice;
            }

            var totalPriceCart = cartItems.Sum(ci => UnitPrice(ci) * ci.Quantity);

            decimal discountCart = 0;
            int? voucherIdCart = null;
            if (!string.IsNullOrEmpty(VoucherCode))
            {
                var voucher = db.Vouchers.FirstOrDefault(v =>
                    v.Code == VoucherCode && v.IsActive == true &&
                    (v.ExpiryDate == null || v.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now)));

                if (voucher != null)
                {
                    if (voucher.MinOrderValue.HasValue && totalPriceCart < voucher.MinOrderValue.Value)
                    {
                        ModelState.AddModelError("", $"Tổng giá trị đơn hàng phải từ {voucher.MinOrderValue.Value:N0}₫ để áp dụng voucher này.");

                        ViewBag.CartItems = cartItems;
                        ViewBag.FullName = user.FullName ?? "";
                        ViewBag.Phone = user.Phone ?? "";
                        ViewBag.Address = user.Address ?? "";
                        ViewBag.Total = totalPriceCart;
                        ViewBag.Discount = 0m;
                        ViewBag.TotalAfterDiscount = totalPriceCart;
                        return View("Checkout");
                    }

                    discountCart = voucher.DiscountType.ToLower() == "percent"
                        ? totalPriceCart * voucher.DiscountValue
                        : voucher.DiscountValue;
                    discountCart = Math.Min(discountCart, totalPriceCart);
                    voucherIdCart = voucher.Id;

                    ViewBag.VoucherType = voucher.DiscountType.ToLower();
                    ViewBag.VoucherValue = voucher.DiscountValue;
                }
                else
                {
                    ModelState.AddModelError("", "Mã voucher không hợp lệ hoặc đã hết hạn.");

                    ViewBag.CartItems = cartItems;
                    ViewBag.FullName = user.FullName ?? "";
                    ViewBag.Phone = user.Phone ?? "";
                    ViewBag.Address = user.Address ?? "";
                    ViewBag.Total = totalPriceCart;
                    ViewBag.Discount = 0m;
                    ViewBag.TotalAfterDiscount = totalPriceCart;
                    return View("Checkout");
                }
            }

            ViewBag.Total = totalPriceCart;
            ViewBag.Discount = discountCart;
            ViewBag.TotalAfterDiscount = totalPriceCart - discountCart;

            // Stock check (correctly check variation stock if applicable)
            foreach (var ci in cartItems)
            {
                int availableStock;
                if (ci.VariantId != null && ci.Product?.ProductVariations != null)
                {
                    var v = ci.Product.ProductVariations.FirstOrDefault(x => x.Id == ci.VariantId.Value);
                    availableStock = v?.Stock ?? 0;
                }
                else
                {
                    availableStock = ci.Product?.Stock ?? 0;
                }

                if (availableStock < ci.Quantity)
                {
                    ModelState.AddModelError("", $"Sản phẩm '{ci.Product?.Name}' chỉ còn {availableStock} sản phẩm trong kho.");

                    ViewBag.CartItems = cartItems;
                    ViewBag.FullName = user.FullName ?? "";
                    ViewBag.Phone = user.Phone ?? "";
                    ViewBag.Address = user.Address ?? "";
                    return View("Checkout");
                }
            }

            var orderCart = new Order
            {
                UserId = user.Id,
                TotalPrice = totalPriceCart - discountCart,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                VoucherId = voucherIdCart,
                Description = Note,
                ShippingName = FullName,
                ShippingPhone = Phone,
                ShippingAddress = Address,
                IsPaid = false // mặc định
            };
            db.Orders.Add(orderCart);
            db.SaveChanges();

            foreach (var ci in cartItems)
            {
                var finalPrice = UnitPrice(ci);
                db.OrderItems.Add(new OrderItem
                {
                    OrderId = orderCart.Id,
                    ProductId = ci.ProductId,
                    VariantId = ci.VariantId,
                    Quantity = ci.Quantity,
                    Price = finalPrice
                });
            }

            db.CartItems.RemoveRange(cartItems);
            db.SaveChanges();

            // Điều hướng theo phương thức thanh toán
            if (!string.IsNullOrEmpty(PaymentMethod) && PaymentMethod.Equals("Bank", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Momo", "Payment", new { id = orderCart.Id });
            }

            TempData["OrderSuccess"] = "Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.";
            return RedirectToAction("OrderDetail", new { id = orderCart.Id });
        }

        // Hiển thị trang checkout cho sản phẩm mua ngay từ trang chi tiết
        [HttpGet]
        public IActionResult CheckoutSingle(int productId, int quantity = 1, int? variationId = null)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = db.Products
                .Include(p => p.ProductVariations)
                .FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                TempData["OrderError"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Detail", "Detail", new { id = productId });
            }

            HandmadeShop.Models.ProductVariation? variation = null;
            decimal price = 0;
            decimal discountRate = product.DiscountedPrice ?? 0;
            string variationText = "Không phân loại";
            if (variationId.HasValue && product.ProductVariations != null)
            {
                variation = product.ProductVariations.FirstOrDefault(v => v.Id == variationId.Value);
                if (variation != null)
                {
                    price = (discountRate > 0) ? variation.Price * (1 - discountRate) : variation.Price;
                    if (!string.IsNullOrEmpty(variation.CombinationHash))
                        variationText = variation.CombinationHash;
                }
                else
                {
                    price = (discountRate > 0) ? (product.Price ?? 0) * (1 - discountRate) : (product.Price ?? 0);
                }
            }
            else
            {
                price = (discountRate > 0) ? (product.Price ?? 0) * (1 - discountRate) : (product.Price ?? 0);
            }

            var cartItem = new HandmadeShop.Models.CartItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = quantity,
                VariantId = variationId
            };
            var cartItems = new List<HandmadeShop.Models.CartItem> { cartItem };

            // Buy-now flags for the view and POST
            ViewBag.IsBuyNow = true;
            ViewBag.BuyNowProductId = product.Id;
            ViewBag.BuyNowQuantity = quantity;
            ViewBag.BuyNowVariationId = variationId;

            ViewBag.CartItems = cartItems;
            ViewBag.FullName = user.FullName ?? "";
            ViewBag.Phone = user.Phone ?? "";
            ViewBag.Address = user.Address ?? "";
            ViewBag.Total = price * quantity;
            ViewBag.TotalAfterDiscount = price * quantity;
            ViewBag.Discount = 0;
            ViewBag.VoucherType = null;
            ViewBag.VoucherValue = null;

            return View("Checkout");
        }

        // Hiển thị trang checkout cho các sản phẩm đã chọn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckoutSelected(int[] selectedItems)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Lấy các sản phẩm được chọn
            var cartItems = db.CartItems
                .Where(ci => ci.UserId == user.Id && selectedItems.Contains(ci.Id))
                .Include(ci => ci.Product)
                .ThenInclude(p => p.ProductVariations)
                .ToList();

            if (!cartItems.Any())
            {
                ModelState.AddModelError("", "Bạn chưa chọn sản phẩm nào để thanh toán.");
                return RedirectToAction("Shopping_cart", "Shopping_cart");
            }

            ViewBag.CartItems = cartItems;
            ViewBag.FullName = user.FullName ?? "";
            ViewBag.Phone = user.Phone ?? "";
            ViewBag.Address = user.Address ?? "";

            // Tính tổng tiền dựa trên giá đã giảm
            ViewBag.Total = cartItems.Sum(ci =>
            {
                decimal price = 0;
                decimal discountRate = ci.Product?.DiscountedPrice ?? 0;
                if (ci.VariantId != null && ci.Product?.ProductVariations != null)
                {
                    var variation = ci.Product.ProductVariations.FirstOrDefault(v => v.Id == ci.VariantId.Value);
                    if (variation != null)
                        price = (discountRate > 0) ? variation.Price * (1 - discountRate) : variation.Price;
                    else
                        price = (discountRate > 0) ? (ci.Product.Price ?? 0) * (1 - discountRate) : (ci.Product.Price ?? 0);
                }
                else
                {
                    price = (discountRate > 0) ? (ci.Product.Price ?? 0) * (1 - discountRate) : (ci.Product.Price ?? 0);
                }
                return price * ci.Quantity;
            });

            // Lưu lại selectedItems để dùng khi đặt hàng (nếu cần)
            TempData["SelectedCartItems"] = string.Join(",", selectedItems);

            return View("Checkout");
        }

        // Áp dụng voucher
        [HttpPost]
        public IActionResult ApplyVoucher(
            string voucherCode,
            bool? isBuyNow,
            int? productId,
            int? quantity,
            int? variationId)
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return Json(new { success = false, message = "Không tìm thấy người dùng!" });

            // BUY-NOW voucher calculation
            if (isBuyNow == true && productId.HasValue && quantity.HasValue)
            {
                var product = db.Products
                    .Include(p => p.ProductVariations)
                    .FirstOrDefault(p => p.Id == productId.Value);
                if (product == null)
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

                decimal discountRate = product.DiscountedPrice ?? 0m;
                decimal unitPrice;
                if (variationId.HasValue)
                {
                    var v = product.ProductVariations.FirstOrDefault(x => x.Id == variationId.Value);
                    if (v == null)
                        return Json(new { success = false, message = "Phân loại sản phẩm không hợp lệ!" });
                    unitPrice = (discountRate > 0) ? v.Price * (1 - discountRate) : v.Price;
                }
                else
                {
                    var basePrice = product.Price ?? 0m;
                    unitPrice = (discountRate > 0) ? basePrice * (1 - discountRate) : basePrice;
                }

                var total = unitPrice * quantity.Value;

                var voucher = db.Vouchers.FirstOrDefault(v =>
                    v.Code == voucherCode && v.IsActive == true &&
                    (v.ExpiryDate == null || v.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now)));

                if (voucher == null)
                    return Json(new { success = false, message = "Mã voucher không hợp lệ hoặc đã hết hạn!" });

                if (voucher.MinOrderValue.HasValue && total < voucher.MinOrderValue.Value)
                    return Json(new { success = false, message = $"Tổng giá trị đơn hàng phải từ {voucher.MinOrderValue.Value:N0}₫ để áp dụng voucher này." });

                decimal discount = voucher.DiscountType.ToLower() == "percent"
                    ? total * voucher.DiscountValue
                    : voucher.DiscountValue;
                discount = Math.Min(discount, total);

                return Json(new
                {
                    success = true,
                    message = "Áp dụng voucher thành công!",
                    discount = discount,
                    total = total,
                    totalAfterDiscount = total - discount,
                    discountFormatted = string.Format("{0:N0}₫", discount),
                    totalFormatted = string.Format("{0:N0}₫", total),
                    totalAfterDiscountFormatted = string.Format("{0:N0}₫", total - discount),
                    voucherType = voucher.DiscountType.ToLower(),
                    voucherValue = voucher.DiscountValue
                });
            }

            // CART voucher calculation (existing)
            // Lấy selectedItems từ TempData nếu có, và giữ lại giá trị cho lần gọi tiếp theo
            int[] selectedItems = Array.Empty<int>();
            if (TempData["SelectedCartItems"] != null)
            {
                selectedItems = TempData["SelectedCartItems"].ToString().Split(',').Select(int.Parse).ToArray();
                TempData.Keep("SelectedCartItems");
            }

            var cartItems = db.CartItems
                .Where(ci => ci.UserId == user.Id && (selectedItems.Length == 0 || selectedItems.Contains(ci.Id)))
                .Include(ci => ci.Product)
                .ThenInclude(p => p.ProductVariations)
                .ToList();

            var totalCart = cartItems.Sum(ci =>
            {
                decimal price = 0;
                decimal discountRate = ci.Product?.DiscountedPrice ?? 0;
                if (ci.VariantId != null && ci.Product?.ProductVariations != null)
                {
                    var variation = ci.Product.ProductVariations.FirstOrDefault(v => v.Id == ci.VariantId.Value);
                    if (variation != null)
                        price = (discountRate > 0) ? variation.Price * (1 - discountRate) : variation.Price;
                    else
                        price = (discountRate > 0) ? (ci.Product.Price ?? 0) * (1 - discountRate) : (ci.Product.Price ?? 0);
                }
                else
                {
                    price = (discountRate > 0) ? (ci.Product.Price ?? 0) * (1 - discountRate) : (ci.Product.Price ?? 0);
                }
                return price * ci.Quantity;
            });

            var voucherCart = db.Vouchers.FirstOrDefault(v =>
                v.Code == voucherCode && v.IsActive == true &&
                (v.ExpiryDate == null || v.ExpiryDate >= DateOnly.FromDateTime(DateTime.Now)));

            if (voucherCart == null)
                return Json(new { success = false, message = "Mã voucher không hợp lệ hoặc đã hết hạn!" });

            if (voucherCart.MinOrderValue.HasValue && totalCart < voucherCart.MinOrderValue.Value)
                return Json(new { success = false, message = $"Tổng giá trị đơn hàng phải từ {voucherCart.MinOrderValue.Value:N0}₫ để áp dụng voucher này." });

            decimal discountCart = voucherCart.DiscountType.ToLower() == "percent"
                ? totalCart * voucherCart.DiscountValue
                : voucherCart.DiscountValue;
            discountCart = Math.Min(discountCart, totalCart);

            return Json(new
            {
                success = true,
                message = "Áp dụng voucher thành công!",
                discount = discountCart,
                total = totalCart,
                totalAfterDiscount = totalCart - discountCart,
                discountFormatted = string.Format("{0:N0}₫", discountCart),
                totalFormatted = string.Format("{0:N0}₫", totalCart),
                totalAfterDiscountFormatted = string.Format("{0:N0}₫", totalCart - discountCart),
                voucherType = voucherCart.DiscountType.ToLower(),
                voucherValue = voucherCart.DiscountValue
            });
        }

        // Xem chi tiết đơn hàng sau khi đặt thành công
        [HttpGet]
        public IActionResult OrderDetail(int id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("Login", "Account");

            var order = db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductVariations)
                .FirstOrDefault(o => o.Id == id && o.UserId == user.Id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
