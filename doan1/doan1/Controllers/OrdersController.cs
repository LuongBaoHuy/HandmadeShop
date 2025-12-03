using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using doan1.Data;
using doan1.Models;
using Microsoft.AspNetCore.Authorization;

namespace doan1.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class OrdersController : Controller
    {
        private readonly Data.HandmadeShopContext _context;

        // Các tên trường ứng viên cho số lượng ở các bảng (tránh phụ thuộc cứng vào tên property)
        // Ưu tiên theo thứ tự khai báo.
        private static readonly string[] VariationQtyProps = new[] { "Stock", "Quantity", "StockQuantity", "AvailableQuantity" };
        private static readonly string[] ProductQtyProps   = new[] { "Quantity", "Stock", "StockQuantity", "AvailableQuantity" };
        private static readonly string[] OrderItemQtyProps = new[] { "Quantity", "Qty", "Count", "Amount" };

        public OrdersController(Data.HandmadeShopContext context)
        {
            _context = context;
        }

        // Trang danh sách đơn hàng
        public async Task<IActionResult> Index(string searchTerm, string status, string priceRange, 
            DateTime? fromDate, DateTime? toDate, string sortBy = "createdat", string sortOrder = "desc", string? paymentStatus = null)
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.PriceRange = priceRange;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            // NEW: giữ state bộ lọc thanh toán
            ViewBag.PaymentStatus = paymentStatus;

            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .AsQueryable();

            // Tìm kiếm theo từ khóa (ID đơn hàng, tên người dùng, email, số điện thoại, địa chỉ)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out int orderId))
                {
                    ordersQuery = ordersQuery.Where(o => o.Id == orderId || 
                        (o.User != null && !string.IsNullOrEmpty(o.User.FullName) && o.User.FullName.Contains(searchTerm)) ||
                        (o.User != null && !string.IsNullOrEmpty(o.User.Email) && o.User.Email.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(o.ShippingName) && o.ShippingName.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(o.ShippingPhone) && o.ShippingPhone.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(o.ShippingAddress) && o.ShippingAddress.Contains(searchTerm)));
                }
                else
                {
                    ordersQuery = ordersQuery.Where(o => 
                        (o.User != null && !string.IsNullOrEmpty(o.User.FullName) && o.User.FullName.Contains(searchTerm)) ||
                        (o.User != null && !string.IsNullOrEmpty(o.User.Email) && o.User.Email.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(o.ShippingName) && o.ShippingName.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(o.ShippingPhone) && o.ShippingPhone.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(o.ShippingAddress) && o.ShippingAddress.Contains(searchTerm)));
                }
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            // NEW: Lọc theo thanh toán
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                if (paymentStatus == "paid")
                {
                    ordersQuery = ordersQuery.Where(o => o.IsPaid == true);
                }
                else if (paymentStatus == "unpaid")
                {
                    ordersQuery = ordersQuery.Where(o => o.IsPaid != true);
                }
            }

            // Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "under-500k":
                        ordersQuery = ordersQuery.Where(o => o.TotalPrice < 500000);
                        break;
                    case "500k-1m":
                        ordersQuery = ordersQuery.Where(o => o.TotalPrice >= 500000 && o.TotalPrice <= 1000000);
                        break;
                    case "1m-5m":
                        ordersQuery = ordersQuery.Where(o => o.TotalPrice > 1000000 && o.TotalPrice <= 5000000);
                        break;
                    case "over-5m":
                        ordersQuery = ordersQuery.Where(o => o.TotalPrice > 5000000);
                        break;
                }
            }

            // Lọc theo khoảng thời gian
            if (fromDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                // Thêm một ngày để bao gồm toàn bộ ngày kết thúc
                var endDate = toDate.Value.AddDays(1);
                ordersQuery = ordersQuery.Where(o => o.CreatedAt < endDate);
            }

            // Áp dụng sắp xếp với xử lý null an toàn
            switch (sortBy.ToLower())
            {
                case "id":
                    ordersQuery = sortOrder == "desc" 
                        ? ordersQuery.OrderByDescending(o => o.Id) 
                        : ordersQuery.OrderBy(o => o.Id);
                    break;
                case "user":
                    ordersQuery = sortOrder == "desc" 
                        ? ordersQuery.OrderByDescending(o => o.User != null ? o.User.FullName ?? "" : "") 
                        : ordersQuery.OrderBy(o => o.User != null ? o.User.FullName ?? "" : "");
                    break;
                case "totalprice":
                    ordersQuery = sortOrder == "desc" 
                        ? ordersQuery.OrderByDescending(o => o.TotalPrice) 
                        : ordersQuery.OrderBy(o => o.TotalPrice);
                    break;
                case "status":
                    ordersQuery = sortOrder == "desc" 
                        ? ordersQuery.OrderByDescending(o => o.Status ?? "") 
                        : ordersQuery.OrderBy(o => o.Status ?? "");
                    break;
                case "createdat":
                default:
                    // Mặc định sắp xếp theo ngày tạo (mới nhất trước)
                    ordersQuery = sortOrder == "desc" 
                        ? ordersQuery.OrderByDescending(o => o.CreatedAt ?? DateTime.MinValue) 
                        : ordersQuery.OrderBy(o => o.CreatedAt ?? DateTime.MinValue);
                    break;
            }

            var orders = await ordersQuery.ToListAsync();
            ViewBag.TotalResults = orders.Count;
            
            return View(orders);
        }

        // Trang chi tiết đơn hàng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            // Load variation option links separately if needed
            if (order.OrderItems?.Any() == true)
            {
                foreach (var orderItem in order.OrderItems.Where(oi => oi.ProductVariation != null))
                {
                    if (orderItem.ProductVariation != null)
                    {
                        await _context.Entry(orderItem.ProductVariation)
                            .Collection(pv => pv.VariationOptionLinks)
                            .Query()
                            .Include(vol => vol.AttributeOption)
                                .ThenInclude(ao => ao.Attribute)
                            .LoadAsync();
                    }
                }
            }

            return View(order);
        }

        // Trang chỉnh sửa đơn hàng
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", order.UserId);
            ViewData["VoucherId"] = new SelectList(_context.Vouchers, "Id", "Code", order.VoucherId);
            return View(order);
        }

        // Xử lý cập nhật đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Status,Description,ShippingName,ShippingPhone,ShippingAddress")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            // Ghi log thông tin được gửi từ form
            Console.WriteLine($"Received Order ID: {order.Id}");
            Console.WriteLine($"Received Status: {order.Status}");
            Console.WriteLine($"Received ShippingName: {order.ShippingName}");
            Console.WriteLine($"Received ShippingPhone: {order.ShippingPhone}");
            Console.WriteLine($"Received ShippingAddress: {order.ShippingAddress}");

            // Loại bỏ lỗi xác thực cho các trường không được chỉnh sửa
            ModelState.Remove("UserId");
            ModelState.Remove("TotalPrice");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("VoucherId");
            ModelState.Remove("User");
            ModelState.Remove("Voucher");

            // Kiểm tra hợp lệ dữ liệu ModelState
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is not valid:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                
                TempData["Error"] = "Có lỗi trong dữ liệu nhập vào. Vui lòng kiểm tra lại!";
                
                // Tải lại thông tin User và Voucher khi có lỗi xác thực
                var orderWithDetails = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Voucher)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);
                
                if (orderWithDetails != null)
                {
                    order.User = orderWithDetails.User;
                    order.Voucher = orderWithDetails.Voucher;
                    order.UserId = orderWithDetails.UserId;
                    order.TotalPrice = orderWithDetails.TotalPrice;
                    order.CreatedAt = orderWithDetails.CreatedAt;
                    order.VoucherId = orderWithDetails.VoucherId;
                }
                
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", order.UserId);
                ViewData["VoucherId"] = new SelectList(_context.Vouchers, "Id", "Code", order.VoucherId);
                return View(order);
            }

            try
            {
                // Lấy đơn hàng hiện tại từ cơ sở dữ liệu
                var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
                if (existingOrder == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng!";
                    return NotFound();
                }

                // Kiểm tra logic chuyển trạng thái
                if (existingOrder.Status == "Completed")
                {
                    TempData["Error"] = "Đơn hàng đã hoàn thành, không thể chỉnh sửa!";
                    return RedirectToAction(nameof(Index));
                }

                if (order.Status == "Completed")
                {
                    TempData["Error"] = "Admin không thể đặt trạng thái 'Hoàn thành'. Chỉ khách hàng mới có thể xác nhận đã nhận hàng!";
                    
                    // Tải lại thông tin để hiển thị form
                    var orderWithDetails = await _context.Orders
                        .Include(o => o.User)
                        .Include(o => o.Voucher)
                        .FirstOrDefaultAsync(o => o.Id == order.Id);
                    
                    if (orderWithDetails != null)
                    {
                        order.User = orderWithDetails.User;
                        order.Voucher = orderWithDetails.Voucher;
                        order.UserId = orderWithDetails.UserId;
                        order.TotalPrice = orderWithDetails.TotalPrice;
                        order.CreatedAt = orderWithDetails.CreatedAt;
                        order.VoucherId = orderWithDetails.VoucherId;
                        order.Status = orderWithDetails.Status; // Giữ nguyên trạng thái cũ
                    }
                    
                    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", order.UserId);
                    ViewData["VoucherId"] = new SelectList(_context.Vouchers, "Id", "Code", order.VoucherId);
                    return View(order);
                }

                // Chỉ cho phép chuyển từ Pending thành Confirmed
                if (existingOrder.Status == "Pending" && order.Status != "Pending" && order.Status != "Confirmed")
                {
                    TempData["Error"] = "Chỉ có thể chuyển từ 'Chờ xử lý' thành 'Đã xác nhận'!";
                    
                    // Tải lại thông tin để hiển thị form
                    var orderWithDetails = await _context.Orders
                        .Include(o => o.User)
                        .Include(o => o.Voucher)
                        .FirstOrDefaultAsync(o => o.Id == order.Id);
                    
                    if (orderWithDetails != null)
                    {
                        order.User = orderWithDetails.User;
                        order.Voucher = orderWithDetails.Voucher;
                        order.UserId = orderWithDetails.UserId;
                        order.TotalPrice = orderWithDetails.TotalPrice;
                        order.CreatedAt = orderWithDetails.CreatedAt;
                        order.VoucherId = orderWithDetails.VoucherId;
                        order.Status = orderWithDetails.Status; // Giữ nguyên trạng thái cũ
                    }
                    
                    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", order.UserId);
                    ViewData["VoucherId"] = new SelectList(_context.Vouchers, "Id", "Code", order.VoucherId);
                    return View(order);
                }

                Console.WriteLine($"Before update - ShippingName: {existingOrder.ShippingName}");
                Console.WriteLine($"Before update - ShippingPhone: {existingOrder.ShippingPhone}");
                Console.WriteLine($"Before update - ShippingAddress: {existingOrder.ShippingAddress}");

                var isPendingToConfirmed = existingOrder.Status == "Pending" && order.Status == "Confirmed";

                // Dùng transaction để đảm bảo tính toàn vẹn giữa trừ kho và đổi trạng thái
                using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    if (isPendingToConfirmed)
                    {
                        var deductResult = await TryDeductStockForOrderAsync(existingOrder.Id);
                        if (!deductResult.Ok)
                        {
                            await tx.RollbackAsync();
                            TempData["Error"] = deductResult.Error;
                            // Tải lại thông tin để hiển thị form
                            var orderWithDetails = await _context.Orders
                                .Include(o => o.User)
                                .Include(o => o.Voucher)
                                .FirstOrDefaultAsync(o => o.Id == order.Id);

                            if (orderWithDetails != null)
                            {
                                order.User = orderWithDetails.User;
                                order.Voucher = orderWithDetails.Voucher;
                                order.UserId = orderWithDetails.UserId;
                                order.TotalPrice = orderWithDetails.TotalPrice;
                                order.CreatedAt = orderWithDetails.CreatedAt;
                                order.VoucherId = orderWithDetails.VoucherId;
                                order.Status = orderWithDetails.Status; // Giữ nguyên trạng thái cũ
                            }

                            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", order.UserId);
                            ViewData["VoucherId"] = new SelectList(_context.Vouchers, "Id", "Code", order.VoucherId);
                            return View(order);
                        }
                    }

                    // Cập nhật các trường
                    existingOrder.Status = order.Status;
                    existingOrder.Description = order.Description;
                    existingOrder.ShippingName = order.ShippingName;
                    existingOrder.ShippingPhone = order.ShippingPhone;
                    existingOrder.ShippingAddress = order.ShippingAddress;

                    var changes = await _context.SaveChangesAsync();
                    Console.WriteLine($"Number of changes saved: {changes}");

                    await tx.CommitAsync();
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    Console.WriteLine($"Error in Edit transaction: {ex.Message}");
                    throw;
                }

                TempData["Success"] = isPendingToConfirmed
                    ? "Xác nhận đơn hàng và trừ kho thành công!"
                    : "Cập nhật thông tin đơn hàng và giao hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(order.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật đơn hàng!";
                
                // Tải lại thông tin User và Voucher khi có lỗi
                var orderWithDetails = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.Voucher)
                    .FirstOrDefaultAsync(o => o.Id == order.Id);
                
                if (orderWithDetails != null)
                {
                    order.User = orderWithDetails.User;
                    order.Voucher = orderWithDetails.Voucher;
                    order.UserId = orderWithDetails.UserId;
                    order.TotalPrice = orderWithDetails.TotalPrice;
                    order.CreatedAt = orderWithDetails.CreatedAt;
                    order.VoucherId = orderWithDetails.VoucherId;
                }
                
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", order.UserId);
                ViewData["VoucherId"] = new SelectList(_context.Vouchers, "Id", "Code", order.VoucherId);
                return View(order);
            }
        }

        // Trang xác nhận xóa đơn hàng
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Xử lý xóa đơn hàng - Sử dụng raw SQL thay thế
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Kiểm tra đơn hàng có tồn tại không
                var orderExists = await _context.Orders.AnyAsync(o => o.Id == id);
                if (!orderExists)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng!";
                    return RedirectToAction(nameof(Index));
                }

                // Sử dụng raw SQL để xóa an toàn hơn
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Xóa các mục đơn hàng trước
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM OrderItems WHERE OrderId = {0}", id);
                    
                    // Xóa đơn hàng sau
                    await _context.Database.ExecuteSqlRawAsync("DELETE FROM Orders WHERE Id = {0}", id);
                    
                    await transaction.CommitAsync();
                    TempData["Success"] = "Xóa đơn hàng thành công!";
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa đơn hàng: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }

        // Xác nhận đã nhận hàng (Khách hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmDelivery(int id)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng!";
                    return RedirectToAction(nameof(Index));
                }

                // Chỉ cho phép xác nhận nếu đơn hàng đã được admin confirm
                if (order.Status != "Confirmed")
                {
                    TempData["Error"] = "Đơn hàng chưa được xác nhận bởi admin hoặc đã được xử lý!";
                    return RedirectToAction(nameof(Index));
                }

                order.Status = "Completed";
                await _context.SaveChangesAsync();

                TempData["Success"] = "Xác nhận đã nhận hàng thành công! Bạn có thể đánh giá sản phẩm.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Xác nhận đơn hàng (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminConfirm(int id)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng!";
                    return RedirectToAction(nameof(Index));
                }

                // Chỉ cho phép xác nhận nếu đơn hàng đang ở trạng thái Pending
                if (order.Status != "Pending")
                {
                    TempData["Error"] = "Đơn hàng không ở trạng thái chờ xử lý!";
                    return RedirectToAction(nameof(Index));
                }

                using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    var deductResult = await TryDeductStockForOrderAsync(order.Id);
                    if (!deductResult.Ok)
                    {
                        await tx.RollbackAsync();
                        TempData["Error"] = deductResult.Error;
                        return RedirectToAction(nameof(Index));
                    }

                    order.Status = "Confirmed";
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    TempData["Error"] = "Có lỗi xảy ra khi xác nhận: " + ex.Message;
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Xác nhận đơn hàng và trừ kho thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ===================== Helpers: Trừ kho & cập nhật tổng số lượng Product =====================

        // Trừ kho theo từng order item (ProductVariation ưu tiên; nếu không có variation thì trừ trực tiếp Product)
        // Sau đó cập nhật Quantity của Product = tổng Quantity các ProductVariation thuộc Product đó.
        private async Task<(bool Ok, string Error)> TryDeductStockForOrderAsync(int orderId)
        {
            // Load các item + Product + ProductVariation
            var items = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Product)
                .Include(oi => oi.ProductVariation)
                .ToListAsync();

            if (items.Count == 0)
            {
                // Không có item thì không cần trừ kho
                return (true, "");
            }

            // Thu thập ProductIds có variation để lát nữa tổng hợp lại
            var affectedProductIdsWithVariations = new HashSet<int>();

            // 1) Kiểm tra tồn kho trước (để không trừ dở dang)
            foreach (var item in items)
            {
                if (!TryReadIntProperty(item, OrderItemQtyProps, out var itemQty, out _))
                    return (false, "Không đọc được số lượng của mục đơn hàng.");

                if (itemQty <= 0) continue;

                if (item.ProductVariation != null)
                {
                    if (!TryReadIntProperty(item.ProductVariation, VariationQtyProps, out var currentStock, out _))
                        return (false, "Không đọc được tồn kho của phân loại sản phẩm.");

                    if (currentStock < itemQty)
                        return (false, "Không đủ tồn kho cho một hoặc nhiều phân loại sản phẩm trong đơn hàng.");

                    // Lưu productId bị ảnh hưởng
                    if (!TryReadIntProperty(item.Product, new[] { "Id" }, out var pid, out _))
                    {
                        // Nếu Product chưa có id qua navigation, thử từ Variation.ProductId
                        if (!TryReadIntProperty(item.ProductVariation, new[] { "ProductId" }, out pid, out _))
                            return (false, "Không xác định được ProductId để cập nhật tổng số lượng.");
                    }
                    affectedProductIdsWithVariations.Add(pid);
                }
                else
                {
                    // Không có variation => trừ trực tiếp vào Product
                    if (item.Product == null)
                        return (false, "Không tìm thấy thông tin sản phẩm cho một mục đơn hàng.");

                    if (!TryReadIntProperty(item.Product, ProductQtyProps, out var currentProductQty, out _))
                        return (false, "Không đọc được tồn kho của sản phẩm.");

                    if (currentProductQty < itemQty)
                        return (false, "Không đủ tồn kho cho một hoặc nhiều sản phẩm trong đơn hàng.");
                }
            }

            // 2) Trừ kho
            foreach (var item in items)
            {
                TryReadIntProperty(item, OrderItemQtyProps, out var itemQty, out _);
                if (itemQty <= 0) continue;

                if (item.ProductVariation != null)
                {
                    if (!TryReadIntProperty(item.ProductVariation, VariationQtyProps, out var currentStock, out var matchedProp))
                        return (false, "Không đọc được tồn kho của phân loại sản phẩm.");

                    var newStock = currentStock - itemQty;
                    if (newStock < 0) newStock = 0;

                    SetIntProperty(item.ProductVariation, matchedProp, newStock);
                }
                else
                {
                    // Không có variation => trừ trực tiếp Product
                    if (!TryReadIntProperty(item.Product, ProductQtyProps, out var currentProductQty, out var matchedProp))
                        return (false, "Không đọc được tồn kho của sản phẩm.");

                    var newQty = currentProductQty - itemQty;
                    if (newQty < 0) newQty = 0;

                    SetIntProperty(item.Product, matchedProp, newQty);
                }
            }

            // Lưu đợt 1 (để việc SUM variations bên DB lấy đúng dữ liệu mới)
            await _context.SaveChangesAsync();

            // 3) Cập nhật tổng số lượng Product = tổng Quantity của các ProductVariation
            if (affectedProductIdsWithVariations.Count > 0)
            {
                // Xác định property số lượng của Variation dùng để SUM (dựa vào entity bất kỳ đã match trước đó)
                var variationQtyPropName = await DetectExistingPropertyOnSetAsync(nameof(doan1.Models.ProductVariation), VariationQtyProps)
                                            ?? VariationQtyProps.First(); // fallback

                var productQtyPropName = await DetectExistingPropertyOnSetAsync(nameof(doan1.Models.Product), ProductQtyProps)
                                            ?? ProductQtyProps.First(); // fallback

                foreach (var pid in affectedProductIdsWithVariations)
                {
                    // SUM số lượng variations theo productId (lấy ra object rồi cộng trong memory để tránh lệch kiểu)
                    var qtyObjects = await _context.ProductVariations
                        .Where(pv => EF.Property<int>(pv, "ProductId") == pid)
                        .Select(pv => EF.Property<object>(pv, variationQtyPropName))
                        .ToListAsync();

                    var totalQty = qtyObjects.Sum(v => Convert.ToInt32(v ?? 0));

                    var product = await _context.Products.FirstOrDefaultAsync(p => EF.Property<int>(p, "Id") == pid);
                    if (product != null)
                    {
                        SetIntProperty(product, productQtyPropName, totalQty);
                    }
                }

                // Lưu đợt 2 sau khi cập nhật tổng số lượng
                await _context.SaveChangesAsync();
            }

            return (true, "");
        }

        // Đọc int property theo danh sách tên ứng viên
        private bool TryReadIntProperty(object entity, string[] candidates, out int value, out string matchedName)
        {
            var entry = _context.Entry(entity);
            foreach (var name in candidates)
            {
                var prop = entry.Metadata.FindProperty(name);
                if (prop != null)
                {
                    // FIX đơn giản
                    value = Convert.ToInt32(entry.Property(name).CurrentValue ?? 0);
                    matchedName = name;
                    return true;
                }
            }
            value = 0;
            matchedName = "";
            return false;
        }

        // Gán int property theo tên đã biết tồn tại
        private void SetIntProperty(object entity, string propertyName, int value)
        {
            var entry = _context.Entry(entity);
            var prop = entry.Metadata.FindProperty(propertyName);
            if (prop == null)
                throw new InvalidOperationException($"Property '{propertyName}' không tồn tại trên entity '{entry.Metadata.Name}'.");

            // Convert đúng kiểu cột đích trước khi gán
            var targetType = Nullable.GetUnderlyingType(prop.ClrType) ?? prop.ClrType;
            object converted =
                targetType == typeof(int) ? value :
                targetType == typeof(long) ? (long)value :
                targetType == typeof(short) ? (short)value :
                targetType == typeof(byte) ? (byte)Math.Clamp(value, byte.MinValue, byte.MaxValue) :
                targetType == typeof(decimal) ? (decimal)value :
                targetType == typeof(double) ? (double)value :
                Convert.ChangeType(value, targetType);

            entry.Property(propertyName).CurrentValue = converted;
        }

        // Tự động phát hiện property thực tế tồn tại trên DbSet theo danh sách ứng viên (lấy 1 cái đầu tiên tồn tại)
        private async Task<string?> DetectExistingPropertyOnSetAsync(string entityClrName, string[] candidates)
        {
            // Dò trong model metadata
            var model = _context.Model.GetEntityTypes();
            var entityType = model.FirstOrDefault(m => m.ClrType.Name == entityClrName);
            if (entityType == null) return null;

            foreach (var name in candidates)
            {
                if (entityType.FindProperty(name) != null)
                    return name;
            }
            return null;
        }
    }
}
