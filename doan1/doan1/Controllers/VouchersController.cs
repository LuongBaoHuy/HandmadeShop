using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan1.Data;
using doan1.Models;
using Microsoft.AspNetCore.Authorization;

namespace doan1.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class VouchersController : Controller
    {
        private readonly Data.HandmadeShopContext _context;

        public VouchersController(Data.HandmadeShopContext context)
        {
            _context = context;
        }

        // Trang danh sách voucher
        public async Task<IActionResult> Index(string search, string discountType, string status,
            string sortBy = "id", string sortOrder = "asc", decimal? minDiscount = null,
            decimal? maxDiscount = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Vouchers.Include(v => v.Orders).AsQueryable();

            // Tìm kiếm theo mã voucher
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(v => v.Code.Contains(search));
            }

            // Lọc theo loại giảm giá
            if (!string.IsNullOrEmpty(discountType))
            {
                query = query.Where(v => v.DiscountType == discountType);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Now);
                switch (status.ToLower())
                {
                    case "active":
                        query = query.Where(v => v.ExpiryDate > currentDate && v.IsActive == true);
                        break;
                    case "expired":
                        query = query.Where(v => v.ExpiryDate <= currentDate);
                        break;
                    case "inactive":
                        query = query.Where(v => v.IsActive == false || v.IsActive == null);
                        break;
                }
            }

            // Lọc theo giá trị giảm giá
            if (minDiscount.HasValue)
            {
                query = query.Where(v => v.DiscountValue >= minDiscount.Value);
            }
            if (maxDiscount.HasValue)
            {
                query = query.Where(v => v.DiscountValue <= maxDiscount.Value);
            }

            // Lọc theo khoảng thời gian
            if (fromDate.HasValue)
            {
                var fromDateOnly = DateOnly.FromDateTime(fromDate.Value);
                query = query.Where(v => v.ExpiryDate >= fromDateOnly);
            }
            if (toDate.HasValue)
            {
                var toDateOnly = DateOnly.FromDateTime(toDate.Value);
                query = query.Where(v => v.ExpiryDate <= toDateOnly);
            }

            // Sắp xếp
            switch (sortBy?.ToLower())
            {
                case "code":
                    query = sortOrder == "desc" ? query.OrderByDescending(v => v.Code) : query.OrderBy(v => v.Code);
                    break;
                case "discountvalue":
                    query = sortOrder == "desc" ? query.OrderByDescending(v => v.DiscountValue) : query.OrderBy(v => v.DiscountValue);
                    break;
                case "expirydate":
                    query = sortOrder == "desc" ? query.OrderByDescending(v => v.ExpiryDate) : query.OrderBy(v => v.ExpiryDate);
                    break;
                case "usagecount":
                    query = sortOrder == "desc" ? query.OrderByDescending(v => v.Orders.Count()) : query.OrderBy(v => v.Orders.Count());
                    break;
                default:
                    query = sortOrder == "desc" ? query.OrderByDescending(v => v.Id) : query.OrderBy(v => v.Id);
                    break;
            }

            var vouchers = await query.ToListAsync();

            // Truyền tham số tìm kiếm về view
            ViewBag.Search = search;
            ViewBag.DiscountType = discountType;
            ViewBag.Status = status;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.MinDiscount = minDiscount;
            ViewBag.MaxDiscount = maxDiscount;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.TotalResults = vouchers.Count;

            return View(vouchers);
        }

        // Trang chi tiết voucher
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                .Include(v => v.Orders)
                .ThenInclude(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        // GET: Vouchers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vouchers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,DiscountValue,DiscountType,MinOrderValue,ExpiryDate,IsActive")] Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra mã voucher đã tồn tại chưa
                    var existingVoucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == voucher.Code);
                    if (existingVoucher != null)
                    {
                        ModelState.AddModelError("Code", "Mã voucher đã tồn tại!");
                        return View(voucher);
                    }

                    _context.Add(voucher);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Tạo voucher thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo voucher: " + ex.Message);
                }
            }
            return View(voucher);
        }

        // GET: Vouchers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        // POST: Vouchers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,DiscountValue,DiscountType,MinOrderValue,ExpiryDate,IsActive")] Voucher voucher)
        {
            if (id != voucher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra mã voucher đã tồn tại chưa (trừ voucher hiện tại)
                    var existingVoucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.Code == voucher.Code && v.Id != voucher.Id);
                    if (existingVoucher != null)
                    {
                        ModelState.AddModelError("Code", "Mã voucher đã tồn tại!");
                        return View(voucher);
                    }

                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật voucher thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.Id))
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
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật voucher: " + ex.Message);
                }
            }
            return View(voucher);
        }

        // GET: Vouchers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                .Include(v => v.Orders)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        // POST: Vouchers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var voucher = await _context.Vouchers
                    .Include(v => v.Orders)
                    .FirstOrDefaultAsync(v => v.Id == id);
                
                if (voucher != null)
                {
                    // Kiểm tra xem có đơn hàng nào đang sử dụng voucher này không
                    if (voucher.Orders != null && voucher.Orders.Any())
                    {
                        TempData["Error"] = $"Không thể xóa voucher này vì có {voucher.Orders.Count} đơn hàng đang sử dụng!";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Vouchers.Remove(voucher);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa voucher thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy voucher!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa voucher: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VoucherExists(int id)
        {
            return _context.Vouchers.Any(e => e.Id == id);
        }
    }
}
