using System.Diagnostics;
using doan1.Models;
using doan1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace doan1.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Data.HandmadeShopContext _context;

        public HomeController(ILogger<HomeController> logger, Data.HandmadeShopContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Đặt class container cho dashboard toàn màn hình
            ViewBag.ContainerClass = "container-fluid";
            
            try
            {
                // Lấy thống kê dashboard
                var currentMonth = DateTime.Now;
                var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                
                ViewBag.TotalOrdersThisMonth = await _context.Orders
                    .Where(o => o.CreatedAt >= startOfMonth)
                    .CountAsync();
                
                ViewBag.TotalRevenueThisMonth = await _context.Orders
                    .Where(o => o.CreatedAt >= startOfMonth && o.Status == "Completed")
                    .SumAsync(o => o.TotalPrice);
                
                ViewBag.TotalProducts = await _context.Products.CountAsync();
                
                ViewBag.TotalUsers = await _context.Users
                    .Where(u => u.Role == "Customer")
                    .CountAsync();
                
                ViewBag.PendingOrdersCount = await _context.Orders
                    .Where(o => o.Status == "Pending")
                    .CountAsync();
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                // Trả về view với giá trị mặc định
                ViewBag.TotalOrdersThisMonth = 0;
                ViewBag.TotalRevenueThisMonth = 0;
                ViewBag.TotalProducts = 0;
                ViewBag.TotalUsers = 0;
                ViewBag.PendingOrdersCount = 0;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var currentMonth = DateTime.Now;
                var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                
                var stats = new
                {
                    totalOrders = await _context.Orders
                        .Where(o => o.CreatedAt >= startOfMonth)
                        .CountAsync(),
                    
                    totalRevenue = await _context.Orders
                        .Where(o => o.CreatedAt >= startOfMonth && o.Status == "Completed")
                        .SumAsync(o => o.TotalPrice),
                    
                    totalProducts = await _context.Products.CountAsync(),
                    
                    totalUsers = await _context.Users
                        .Where(u => u.Role == "Customer")
                        .CountAsync()
                };
                
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return Json(new { error = "Không thể tải thống kê" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingOrders()
        {
            try
            {
                var pendingOrders = await _context.Orders
                    .Include(o => o.User)
                    .Where(o => o.Status == "Pending")
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new
                    {
                        id = o.Id,
                        customerName = o.User != null ? o.User.FullName ?? o.User.Username : "N/A",
                        totalPrice = o.TotalPrice,
                        createdAt = o.CreatedAt
                    })
                    .ToListAsync();
                
                return Json(pendingOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending orders");
                return Json(new { error = "Không thể tải đơn hàng chờ xử lý" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLowStockProducts()
        {
            try
            {
                var lowStockProducts = await _context.Products
                    .Where(p => p.Stock <= 5)
                    .OrderBy(p => p.Stock)
                    .Take(5)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        stock = p.Stock
                    })
                    .ToListAsync();
                
                return Json(lowStockProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products");
                return Json(new { error = "Không thể tải sản phẩm sắp hết hàng" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetExpiringVouchers()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var nextWeek = today.AddDays(7);

                var expiringVouchers = await _context.Vouchers
                    .Where(v => v.IsActive == true
                                && v.ExpiryDate.HasValue
                                && v.ExpiryDate.Value > today
                                && v.ExpiryDate.Value <= nextWeek)
                    .OrderBy(v => v.ExpiryDate)
                    .Take(5)
                    .Select(v => new
                    {
                        id = v.Id,
                        code = v.Code,
                        // Nếu serializer của bạn không hỗ trợ DateOnly, đổi sang string:
                        // expiryDate = v.ExpiryDate.HasValue ? v.ExpiryDate.Value.ToString("yyyy-MM-dd") : null
                        expiryDate = v.ExpiryDate
                    })
                    .ToListAsync();

                return Json(expiringVouchers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring vouchers");
                return Json(new { error = "Không thể tải voucher sắp hết hạn" });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
