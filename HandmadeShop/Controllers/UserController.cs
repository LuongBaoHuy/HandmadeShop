using System.Text;
using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.IO;
using HandmadeShop.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; 

namespace HandmadeShop.Controllers
{
    public class UserController : BaseController
    {
        private readonly ILogger<UserController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _cache;

        public UserController(HandmadeShopContext context, ILogger<UserController> logger, IEmailSender emailSender, IMemoryCache cache) : base(context)
        {
            _logger = logger;
            _emailSender = emailSender;
            _cache = cache;
        }

        // ===================== AUTHENTICATION =====================
        /// Hiển thị trang đăng nhập/đăng ký.
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Purchase()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "User");

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                return RedirectToAction("Login", "User");

            // Lấy danh sách đơn hàng của người dùng đã đăng nhập
            int userId = int.Parse(userIdClaim.Value);
            var orders = db.Orders
                .Where(o => o.UserId == userId && (o.Status == "Pending" || o.Status == "Confirmed"))
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        /// Xử lý đăng nhập người dùng.
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("LoginError", "Tên người dùng và mật khẩu là bắt buộc.");
                return View();
            }

            var user = db.Users.FirstOrDefault(u => u.Username.ToLower() == username.ToLower());
            if (user != null)
            {
                string hashedPassword = HashPassword(user.Username, password);
                // Chỉ cho phép đăng nhập nếu Role là "User"
                if (user.Password == hashedPassword && (user.Role != null && user.Role == "User"))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim("UserId", user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role ?? "")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                    var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        IsPersistent = false
                    };
                    await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    TempData["Message"] = "Đăng nhập thành công! Chào mừng bạn trở lại, " + user.Username + "!";
                    TempData["MessageType"] = "success";
                    return RedirectToAction("Index", "Home");
                }
            }
            // Nếu sai tên, mật khẩu hoặc không phải Role User thì đều báo lỗi chung
            TempData["Message"] = "Tên người dùng hoặc mật khẩu không hợp lệ! Vui lòng thử lại!.";
            TempData["MessageType"] = "error";
            return View();
        }

        /// Xử lý đăng ký tài khoản mới.
        [HttpPost]
        public IActionResult Register(string email, string username, string phoneNumber, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(phoneNumber) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ModelState.AddModelError("RegisterError", "Vui lòng nhập đầy đủ tất cả các trường.");
                return View("Login");
            }

            // Kiểm tra số điện thoại chỉ chứa số và tối đa 11 ký tự
            if (!phoneNumber.All(char.IsDigit))
            {
                ModelState.AddModelError("RegisterError", "Số điện thoại chỉ được chứa số.");
                return View("Login");
            }
            if (phoneNumber.Length > 11)
            {
                ModelState.AddModelError("RegisterError", "Số điện thoại không hợp lệ.");
                return View("Login");
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("RegisterError", "Mật khẩu xác nhận không khớp.");
                return View("Login");
            }
            if (db.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("RegisterError", "Email đã tồn tại.");
                return View("Login");
            }
            if (db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("RegisterError", "Tên người dùng đã tồn tại. Vui lòng chọn tên khác.");
                return View("Login");
            }
            var newUser = new User
            {
                Username = username,
                Password = HashPassword(username, password),
                Email = email,
                FullName = username,
                Address = "",
                Phone = phoneNumber,
                Role = "User", // Mặc định là User
            };
            db.Users.Add(newUser);
            db.SaveChanges();
            TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Login");
        }

        // ===================== USER INFO =====================
        /// Hiển thị trang thông tin người dùng (yêu cầu đăng nhập).
        public IActionResult Userpage()
        {
            // Kiểm tra xem người dùng đã đăng nhập chưa
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "User");
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "User");
            }
            int userId = int.Parse(userIdClaim.Value);
            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }
            return View(user); // Mặc định trả về view Userpage.cshtml
        }

        /// Cập nhật thông tin cá nhân và đổi mật khẩu (yêu cầu đăng nhập).
        [HttpPost]
        public IActionResult Update(int id, string fullName, string phone, IFormFile ProfileImage, string currentPassword, string newPassword, string confirmPassword)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "User");

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                return RedirectToAction("Login", "User");

            int userId = int.Parse(userIdClaim.Value);
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return RedirectToAction("Login", "User");

            // Cập nhật tên và số điện thoại
            user.FullName = fullName;
            user.Phone = phone;

            // Xử lý upload ảnh đại diện
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Kiểm tra dung lượng và định dạng
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(ProfileImage.FileName).ToLower();
                if (!allowedExtensions.Contains(ext))
                {
                    TempData["Message"] = "Chỉ cho phép tải lên file .jpg, .jpeg, .png";
                    TempData["MessageType"] = "danger";
                    return RedirectToAction("Userpage");
                }
                if (ProfileImage.Length > 5L * 1024 * 1024)
                {
                    TempData["Message"] = "Dung lượng file tối đa là 5MB.";
                    TempData["MessageType"] = "danger";
                    return RedirectToAction("Userpage");
                }

                // Tạo tên file duy nhất
                var fileName = $"avatar_{user.Id}_{DateTime.Now.Ticks}{ext}";
                var savePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "ShareUploads", "Avartar","Customers", fileName);

                // Đảm bảo thư mục tồn tại
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    ProfileImage.CopyTo(stream);
                }

                // Đường dẫn truy cập từ web
                user.ProfileImageUrl = $"/uploads/Avartar/Customers/{fileName}";
            }

            // Kiểm tra và cập nhật Password nếu có
            if (!string.IsNullOrEmpty(newPassword) && !string.IsNullOrEmpty(confirmPassword))
            {
                if (newPassword != confirmPassword)
                {
                    TempData["Message"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Userpage");
                }

                // Kiểm tra currentPassword
                string hashedCurrentPassword = HashPassword(user.Username, currentPassword);
                if (user.Password != hashedCurrentPassword)
                {
                    TempData["Message"] = "Mật khẩu hiện tại không đúng.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Userpage");
                }

                // Cập nhật password mới
                user.Password = HashPassword(user.Username, newPassword);
            }

            db.SaveChanges();

            TempData["Message"] = "Cập nhật thông tin thành công!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Userpage");
        }

        /// Đổi mật khẩu (yêu cầu đăng nhập).
        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "User");

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                return RedirectToAction("Login", "User");

            int userId = int.Parse(userIdClaim.Value);
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return RedirectToAction("Login", "User");

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                TempData["Message"] = "Vui lòng nhập đầy đủ thông tin.";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Userpage");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Message"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Userpage");
            }

            string hashedCurrentPassword = HashPassword(user.Username, currentPassword);
            if (user.Password != hashedCurrentPassword)
            {
                TempData["Message"] = "Mật khẩu hiện tại không đúng.";
                TempData["MessageType"] = "danger";
                return RedirectToAction("Userpage");
            }

            user.Password = HashPassword(user.Username, newPassword);
            db.SaveChanges();

            TempData["Message"] = "Đổi mật khẩu thành công!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Userpage");
        }

        // ===================== LOGOUT =====================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ===================== HELPER =====================
        /// Hàm hash password bằng SHA256 với salt và username.
        private string HashPassword(string username, string password, string salt = "HandmadeShopSalt2025")
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] combinedBytes = System.Text.Encoding.UTF8.GetBytes(username + salt + password);
                byte[] hashedBytes = sha256.ComputeHash(combinedBytes);
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpper(); // Trả về hex uppercase
            }
        }

        // ===================== ORDER =====================
        /// Đánh dấu đơn hàng là đã hoàn thành (yêu cầu đăng nhập).
        [HttpPost]
        public IActionResult MarkAsCompleted(int orderId)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "User");

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null)
                return RedirectToAction("Login", "User");

            int userId = int.Parse(userIdClaim.Value);
            var order = db.Orders.FirstOrDefault(o => o.Id == orderId && o.UserId == userId && o.Status == "Confirmed");
            if (order != null)
            {
                order.Status = "Completed";
                db.SaveChanges();
                TempData["Message"] = "Đơn hàng đã được xác nhận hoàn thành!";
                TempData["MessageType"] = "success";

                // Lấy sản phẩm đầu tiên trong đơn hàng
                var firstOrderItem = db.OrderItems.FirstOrDefault(oi => oi.OrderId == order.Id);
                if (firstOrderItem != null)
                {
                    // Chuyển hướng sang trang chi tiết sản phẩm và mở tab liên hệ
                    return Redirect($"/Detail/Detail/{firstOrderItem.ProductId}?tab=contact");
                }
            }
            return RedirectToAction("Purchase");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập email.");
                return View();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                var code = GenerateOtp();

                // Lưu OTP vào MemoryCache trong 10 phút
                var cacheKey = $"pwdotp:{email.ToLower()}";
                var entry = new OtpEntry
                {
                    Code = code,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    Attempts = 0
                };
                _cache.Set(cacheKey, entry, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

                var subject = "Mã OTP đặt lại mật khẩu";
                var body = $@"<p>Xin chào {user.Username},</p>
                              <p>Mã OTP khôi phục mật khẩu của bạn là: <strong style='font-size:18px'>{code}</strong></p>
                              <p>Mã có hiệu lực trong 10 phút.</p>
                              <p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>";

                await _emailSender.SendAsync(email, subject, body);
            }

            TempData["Message"] = "Nếu email hợp lệ, mã OTP sẽ được gửi.";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(ResetPassword), new { email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string code, string password, string confirmPassword)
        {
            ViewBag.Email = email;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");
                return View();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email không hợp lệ.");
                return View();
            }

            var cacheKey = $"pwdotp:{email.ToLower()}";
            if (!_cache.TryGetValue(cacheKey, out OtpEntry entry))
            {
                ModelState.AddModelError(string.Empty, "OTP đã hết hạn. Vui lòng yêu cầu mã mới.");
                return View();
            }

            if (DateTime.UtcNow > entry.ExpiresAt)
            {
                _cache.Remove(cacheKey);
                ModelState.AddModelError(string.Empty, "OTP đã hết hạn. Vui lòng yêu cầu mã mới.");
                return View();
            }

            if (!string.Equals(entry.Code, code))
            {
                entry.Attempts++;
                // có thể khóa nếu sai quá 5 lần
                if (entry.Attempts >= 5)
                {
                    _cache.Remove(cacheKey);
                    ModelState.AddModelError(string.Empty, "Nhập sai OTP quá số lần cho phép. Vui lòng yêu cầu mã mới.");
                    return View();
                }
                _cache.Set(cacheKey, entry, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = entry.ExpiresAt
                });
                ModelState.AddModelError(string.Empty, "OTP không đúng.");
                return View();
            }

            // Đúng OTP
            user.Password = HashPassword(user.Username, password);
            await db.SaveChangesAsync();

            _cache.Remove(cacheKey); // vô hiệu hóa OTP sau khi dùng

            TempData["Message"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập.";
            TempData["MessageType"] = "success";
            return RedirectToAction("Login", "User");
        }

        private static string GenerateOtp()
        {
            var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return value.ToString("D6");
        }

        // Lưu tạm OTP trong cache
        private class OtpEntry
        {
            public string Code { get; set; } = "";
            public DateTime ExpiresAt { get; set; }
            public int Attempts { get; set; }
        }
    }
}
