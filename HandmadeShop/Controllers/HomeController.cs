using System.Diagnostics;
using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HandmadeShop.Controllers
{
    public class HomeController : BaseController
    {
        private readonly HandmadeShopContext db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(HandmadeShopContext context, ILogger<HomeController> logger) : base(context)
        {
            db = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? page)
        {
            try
            {
                // Số sản phẩm mỗi trang
                int pageSize = 9;
                int pageNumber = (page ?? 1);

                // Lấy danh sách sản phẩm có phân trang
                var products = await db.Products
                    .Include(p => p.Reviews) 
                    .OrderByDescending(p => p.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Lấy tổng số sản phẩm để phân trang
                int totalItems = await db.Products.CountAsync();
                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Lưu thông tin phân trang vào ViewBag
                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPrevious = pageNumber > 1;
                ViewBag.HasNext = pageNumber < totalPages;
                ViewBag.TotalItems = totalItems;

                if (products == null || !products.Any())
                {
                    ViewBag.ErrorMessage = "Không tìm thấy sản phẩm.";
                    return View(new List<Product>());
                }

                // Lấy các danh mục
                ViewBag.Categories = await db.Categories.ToListAsync();
                
                // Lấy sản phẩm hot deals (giảm giá nhiều nhất)
                ViewBag.HotDeals = await db.Products
                    .OrderByDescending(p => p.Price * 0.8m) // Giả sử giảm 20%
                    .Take(3)
                    .ToListAsync();

                // Lấy sản phẩm đặc biệt (giá thấp nhất)
                ViewBag.SpecialOffers = await db.Products
                    .OrderBy(p => p.Price)
                    .Take(3)
                    .ToListAsync();

                // Lấy sản phẩm nổi bật (bán chạy nhất)
                ViewBag.FeaturedProducts = await db.Products
                    .Include(p => p.Reviews) 
                    .OrderByDescending(p => db.OrderItems.Where(oi => oi.ProductId == p.Id).Sum(oi => (int?)oi.Quantity) ?? 0)
                    .Take(6)
                    .ToListAsync();

                // Lấy sản phẩm bán chạy (số lượng tồn kho ít nhất)
                ViewBag.BestSellers = await db.Products
                    .OrderBy(p => p.Stock)
                    .Take(6)
                    .ToListAsync();

                // Lấy sản phẩm mới nhất
                ViewBag.NewArrivals = await db.Products
                    .OrderByDescending(p => p.Id)
                    .Take(6)
                    .ToListAsync();

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu: {Message}", ex.Message);
                ViewBag.ErrorMessage = "Lỗi khi lấy dữ liệu: " + ex.Message;
                return View(new List<Product>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui l�ng ??ng nh?p ?? th�m s?n ph?m v�o gi? h�ng!", redirect = Url.Action("Login", "Account") });
            }

            try
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                if (user == null)
                {
                    return Json(new { success = false, message = "Kh�ng t�m th?y ng??i d�ng!" });
                }
                var userId = user.Id;

                var product = await db.Products.FindAsync(productId);
                if (product == null || product.Stock < quantity)
                {
                    return Json(new { success = false, message = "S?n ph?m kh�ng h?p l? ho?c kh�ng ?? s? l??ng!" });
                }

                var cartItem = await db.CartItems.FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    cartItem = new CartItem
                    {
                        UserId = userId,
                        ProductId = productId,
                        Quantity = quantity,
                        AddedAt = DateTime.Now
                    };
                    db.CartItems.Add(cartItem);
                }

                await db.SaveChangesAsync();

                var cartCount = await db.CartItems.Where(ci => ci.UserId == userId).SumAsync(ci => ci.Quantity);
                var totalPrice = await db.CartItems.Where(ci => ci.UserId == userId).SumAsync(ci => ci.Product.Price * ci.Quantity);

                return Json(new { success = true, message = "?� th�m v�o gi? h�ng!", cartCount = cartCount, totalPrice = totalPrice });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi th�m v�o gi? h�ng: {Message}", ex.Message);
                return Json(new { success = false, message = "C� l?i x?y ra khi th�m v�o gi? h�ng!" });
            }
        }

        public IActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
