using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace HandmadeShop.Controllers
{
    public class DetailController : BaseController
    {
        private readonly ILogger<DetailController> _logger;

        public DetailController(HandmadeShopContext context, ILogger<DetailController> logger) : base(context)
        {
            _logger = logger;
        }

        public IActionResult Detail(int id)
        {
            try
            {
                // Lấy thông tin sản phẩm với các dữ liệu liên quan
                var product = db.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductAttributeOptions)
                        .ThenInclude(pao => pao.AttributeOption)
                            .ThenInclude(ao => ao.Attribute)
                    .Include(p => p.ProductVariations)
                        .ThenInclude(pv => pv.VariationOptionLinks)
                            .ThenInclude(vol => vol.AttributeOption)
                                .ThenInclude(ao => ao.Attribute)
                    .FirstOrDefault(p => p.Id == id);

                if (product == null)
                {
                    return NotFound("Không tìm thấy sản phẩm");
                }

                // Sắp xếp ảnh: IsMain == true lên đầu
                product.ProductImages = product.ProductImages
                    .OrderByDescending(img => img.IsMain == true)
                    .ThenBy(img => img.Id)
                    .ToList();

                // Lấy các thuộc tính động của sản phẩm (Màu sắc, Kích thước...)
                var productAttributes = product.ProductAttributeOptions
                    .Select(pao => pao.AttributeOption.Attribute)
                    .Distinct()
                    .ToList();

                // Lấy tất cả options của các attributes
                var attributeOptions = new Dictionary<int, List<AttributeOption>>();
                foreach (var attr in productAttributes)
                {
                    var options = product.ProductAttributeOptions
                        .Where(pao => pao.AttributeOption.AttributeId == attr.Id)
                        .Select(pao => pao.AttributeOption)
                        .ToList();
                    attributeOptions[attr.Id] = options;
                }

                // Chuẩn bị dữ liệu variations cho JavaScript
                var productDiscountRate = product.DiscountedPrice ?? 0;
                var variationsData = product.ProductVariations.Select(pv => new
                {
                    Id = pv.Id,
                    // Áp dụng giảm giá nếu sản phẩm cha có DiscountedPrice
                    Price = productDiscountRate > 0 ? pv.Price * (1 - productDiscountRate) : pv.Price,
                    Stock = pv.Stock,
                    CombinationHash = pv.CombinationHash,
                    Options = pv.VariationOptionLinks.Select(vol => new
                    {
                        AttributeId = vol.AttributeOption.AttributeId,
                        AttributeName = vol.AttributeOption.Attribute.Name,
                        OptionValue = vol.AttributeOption.Value
                    }).ToList()
                }).ToList();

                // Lấy đánh giá sản phẩm
                var reviews = db.Reviews
                    .Where(r => r.ProductId == id)
                    .Include(r => r.User)
                    .Include(r => r.ReviewReplies)
                        .ThenInclude(rr => rr.User)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                // Tính toán điểm đánh giá trung bình
                var avgRating = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0;
                var totalReviews = reviews.Count;

                // Truyền dữ liệu qua ViewBag
                ViewBag.ProductAttributes = productAttributes;
                ViewBag.AttributeOptions = attributeOptions;
                ViewBag.VariationsData = JsonSerializer.Serialize(variationsData);
                ViewBag.Reviews = reviews;
                ViewBag.AvgRating = avgRating;
                ViewBag.TotalReviews = totalReviews;
                ViewBag.HasVariations = product.ProductVariations.Any();

                // Kiểm tra quyền đánh giá
                bool canReview = false;
                if (User.Identity.IsAuthenticated)
                {
                    var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                    if (user != null)
                    {
                        canReview = db.OrderItems
                            .Include(oi => oi.Order)
                            .Any(oi =>
                                oi.ProductId == id &&
                                oi.Order.UserId == user.Id &&
                                //Trạng thái đơn hàng phải là "Completed"(Đã hoàn thành)
                                (oi.Order.Status == "Completed")
                            );
                    }
                }
                ViewBag.CanReview = canReview;

                if (User.Identity.IsAuthenticated)
                {
                    var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                    if (currentUser != null)
                    {
                        var contactMessages = db.Questions
                            .Where(q => q.UserId == currentUser.Id && q.ProductId == id)
                            .Include(q => q.Answers)
                                .ThenInclude(a => a.User)
                            .Include(q => q.User)
                            .OrderBy(q => q.CreatedAt)
                            .ToList();
                        ViewBag.ContactMessages = contactMessages;
                    }
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chi tiết sản phẩm {ProductId}", id);
                return StatusCode(500, "Có lỗi xảy ra khi tải trang sản phẩm");
            }
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1, int? variationId = null)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (!User.Identity.IsAuthenticated)
                {
                    return Json(new { 
                        success = false, 
                        message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng!",
                        requireLogin = true
                    });
                }

                // Lấy thông tin user
                var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng!" });
                }

                // Lấy thông tin sản phẩm
                var product = db.Products
                    .Include(p => p.ProductVariations)
                    .FirstOrDefault(p => p.Id == productId);

                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
                }

                // Kiểm tra số lượng và stock
                if (quantity <= 0)
                {
                    return Json(new { success = false, message = "Số lượng không hợp lệ!" });
                }

                decimal price = product.Price ?? 0;
                decimal discountRate = product.DiscountedPrice ?? 0;
                int availableStock = product.Stock ?? 0;

                // Nếu có variation, kiểm tra variation
                ProductVariation? selectedVariation = null;
                if (variationId.HasValue)
                {
                    selectedVariation = product.ProductVariations.FirstOrDefault(v => v.Id == variationId.Value);
                    if (selectedVariation == null)
                    {
                        return Json(new { success = false, message = "Phân loại sản phẩm không hợp lệ!" });
                    }
                    price = selectedVariation.Price;
                    availableStock = selectedVariation.Stock;
                }
                else
                {
                    // Nếu không có variation, áp dụng giảm giá cho sản phẩm chính
                    if (discountRate > 0)
                    {
                        price = price * (1 - discountRate);
                    }
                }

                // Kiểm tra tồn kho
                if (availableStock < quantity)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Chỉ còn {availableStock} sản phẩm trong kho!" 
                    });
                }

                // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
                var existingCartItem = db.CartItems.FirstOrDefault(ci => 
                    ci.UserId == user.Id && 
                    ci.ProductId == productId && 
                    ci.VariantId == variationId);

                if (existingCartItem != null)
                {
                    // Kiểm tra tổng số lượng sau khi cập nhật
                    int newQuantity = existingCartItem.Quantity + quantity;
                    if (newQuantity > availableStock)
                    {
                        return Json(new { 
                            success = false, 
                            message = $"Số lượng vượt quá tồn kho! Hiện tại bạn đã có {existingCartItem.Quantity} sản phẩm trong giỏ hàng." 
                        });
                    }
                    existingCartItem.Quantity = newQuantity;
                }
                else
                {
                    // Thêm mới vào giỏ hàng
                    var newCartItem = new CartItem
                    {
                        UserId = user.Id,
                        ProductId = productId,
                        VariantId = variationId,
                        Quantity = quantity,
                        AddedAt = DateTime.Now
                    };
                    db.CartItems.Add(newCartItem);
                }

                db.SaveChanges();

                // Tính toán thông tin giỏ hàng mới
                var cartCount = db.CartItems
                    .Where(ci => ci.UserId == user.Id)
                    .Sum(ci => ci.Quantity);

                return Json(new { 
                    success = true, 
                    message = "Đã thêm sản phẩm vào giỏ hàng thành công!",
                    cartCount = cartCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng: ProductId={ProductId}, Quantity={Quantity}", 
                    productId, quantity);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm vào giỏ hàng!" });
            }
        }

        // Kiểm tra tính khả dụng của variation
        [HttpPost]
        public IActionResult CheckVariationAvailability([FromBody] Dictionary<int, string> selectedOptions)
        {
            try
            {
                if (selectedOptions == null || !selectedOptions.Any())
                {
                    return Json(new { success = false, message = "Chưa chọn phân loại" });
                }

                // Tạo hash từ selected options
                var sortedOptions = selectedOptions
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => $"{kvp.Key}:{kvp.Value}")
                    .ToArray();
                var combinationHash = string.Join("|", sortedOptions);

                // Tìm variation tương ứng
                var variation = db.ProductVariations
                    .FirstOrDefault(pv => pv.CombinationHash == combinationHash);

                if (variation == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Phân loại này hiện không có sẵn" 
                    });
                }

                return Json(new { 
                    success = true,
                    variationId = variation.Id,
                    price = variation.Price,
                    stock = variation.Stock,
                    formattedPrice = variation.Price.ToString("N0") + " VNĐ"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra variation");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }

        // Thêm đánh giá sản phẩm
        [HttpPost]
        public IActionResult AddReview(int productId, int rating, string content)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Kiểm tra đã mua hàng chưa
                var hasPurchased = db.OrderItems
                    .Include(oi => oi.Order)
                    .Any(oi =>
                        oi.ProductId == productId &&
                        oi.Order.UserId == user.Id &&
                        (oi.Order.Status == "Completed" || oi.Order.Status == "Delivered")
                    );
                if (!hasPurchased)
                {
                    TempData["ErrorMessage"] = "Bạn chỉ có thể đánh giá khi đã mua sản phẩm này!";
                    return RedirectToAction("Detail", new { id = productId });
                }

                // Kiểm tra user đã đánh giá sản phẩm này chưa
                var existingReview = db.Reviews.FirstOrDefault(r => r.ProductId == productId && r.UserId == user.Id);
                if (existingReview != null)
                {
                    TempData["ErrorMessage"] = "Bạn đã đánh giá sản phẩm này rồi!";
                    return RedirectToAction("Detail", new { id = productId });
                }

                var review = new Review
                {
                    ProductId = productId,
                    UserId = user.Id,
                    Rating = rating,
                    Comment = content?.Trim(),
                    CreatedAt = DateTime.Now
                };

                db.Reviews.Add(review);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đánh giá của bạn đã được gửi thành công!";
                return RedirectToAction("Detail", new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm đánh giá sản phẩm");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi đánh giá!";
                return RedirectToAction("Detail", new { id = productId });
            }
        }

        // Trả lời đánh giá (chỉ dành cho Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult AddReviewReply(int reviewId, string replyContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(replyContent))
                {
                    TempData["ErrorMessage"] = "Nội dung phản hồi không được để trống!";
                    return RedirectToAction("Detail");
                }

                var review = db.Reviews.FirstOrDefault(r => r.Id == reviewId);
                if (review == null)
                {
                    return NotFound();
                }

                var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var reply = new ReviewReply
                {
                    ReviewId = reviewId,
                    UserId = user.Id,
                    Reply = replyContent.Trim(),
                    CreatedAt = DateTime.Now
                };

                db.ReviewReplies.Add(reply);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Phản hồi đã được gửi thành công!";
                return RedirectToAction("Detail", new { id = review.ProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm phản hồi đánh giá");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi phản hồi!";
                return RedirectToAction("Detail");
            }
        }

        // Thêm method để debug variations
        [HttpGet]
        public IActionResult DebugVariations(int productId)
        {
            var product = db.Products
                .Include(p => p.ProductVariations)
                    .ThenInclude(pv => pv.VariationOptionLinks)
                        .ThenInclude(vol => vol.AttributeOption)
                            .ThenInclude(ao => ao.Attribute)
                .FirstOrDefault(p => p.Id == productId);

            if (product == null) return NotFound();

            var debugInfo = product.ProductVariations.Select(pv => new
            {
                Id = pv.Id,
                CombinationHash = pv.CombinationHash,
                Price = pv.Price,
                Stock = pv.Stock,
                Options = pv.VariationOptionLinks.Select(vol => new
                {
                    AttributeId = vol.AttributeOption.AttributeId,
                    AttributeName = vol.AttributeOption.Attribute.Name,
                    OptionValue = vol.AttributeOption.Value
                }).ToList()
            }).ToList();

            return Json(debugInfo);
        }

        // Gửi câu hỏi hoặc phản hồi Liên hệ
        [HttpPost]
        public IActionResult SendContact(int productId, string content)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");
            var user = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!User.IsInRole("Admin"))
            {
                // User gửi câu hỏi
                var question = new Question
                {
                    UserId = user.Id,
                    ProductId = productId,
                    Content = content,
                    CreatedAt = DateTime.Now,
                    Status = "Pending"
                };
                db.Questions.Add(question);
            }
            else
            {
                // Admin trả lời câu hỏi gần nhất của user này với sản phẩm này
                var lastQuestion = db.Questions
                    .Where(q => q.ProductId == productId)
                    .OrderByDescending(q => q.CreatedAt)
                    .FirstOrDefault();
                if (lastQuestion != null)
                {
                    var answer = new Answer
                    {
                        QuestionId = lastQuestion.Id,
                        UserId = user.Id,
                        Content = content,
                        CreatedAt = DateTime.Now
                    };
                    db.Answers.Add(answer);
                }
            }
            db.SaveChanges();
            return RedirectToAction("Detail", new { id = productId });
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
