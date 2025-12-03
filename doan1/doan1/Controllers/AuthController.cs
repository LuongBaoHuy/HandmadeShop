using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan1.Data;
using doan1.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace doan1.Controllers
{
    public class AuthController : Controller
    {
        private readonly Data.HandmadeShopContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(Data.HandmadeShopContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    ViewBag.Error = "Vui lòng nhập đầy đủ thông tin đăng nhập";
                    return View();
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

                if (user == null)
                {
                    ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác";
                    return View();
                }

                // Kiểm tra quyền truy cập
                if (user.Role != "Admin" && user.Role != "Manager")
                {
                    ViewBag.Error = "Bạn không có quyền truy cập vào hệ thống quản trị";
                    return View();
                }

                // Tạo claims cho người dùng
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("FullName", user.FullName ?? user.Username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation($"User {username} ({user.Role}) logged in successfully");

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ViewBag.Error = "Có lỗi xảy ra trong quá trình đăng nhập";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // Hành động tạm thời để tạo user mẫu - Xóa khi triển khai thực tế
        [HttpGet]
        public async Task<IActionResult> CreateSampleUsers()
        {
            try
            {
                // Kiểm tra xem admin đã tồn tại chưa
                var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (existingAdmin != null)
                {
                    return Json(new { message = "Sample users already exist" });
                }

                var sampleUsers = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        Password = "admin123", // Khi triển khai thực tế, hãy dùng mật khẩu đã mã hóa
                        FullName = "Quản trị viên",
                        Email = "admin@handmadeshop.com",
                        Role = "Admin"
                    },
                    new User
                    {
                        Username = "manager",
                        Password = "manager123", // Khi triển khai thực tế, hãy dùng mật khẩu đã mã hóa
                        FullName = "Quản lý",
                        Email = "manager@handmadeshop.com",
                        Role = "Manager"
                    },
                    new User
                    {
                        Username = "customer",
                        Password = "customer123", // Khi triển khai thực tế, hãy dùng mật khẩu đã mã hóa
                        FullName = "Khách hàng",
                        Email = "customer@handmadeshop.com",
                        Role = "Customer"
                    }
                };

                _context.Users.AddRange(sampleUsers);
                await _context.SaveChangesAsync();

                return Json(new { 
                    message = "Tạo người dùng mẫu thành công",
                    users = new {
                        admin = "admin/admin123",
                        manager = "manager/manager123", 
                        customer = "customer/customer123 (không có quyền admin)"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sample users");
                return Json(new { error = "Error creating sample users" });
            }
        }
    }
}
