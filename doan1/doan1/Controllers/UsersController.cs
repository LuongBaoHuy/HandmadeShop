using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan1.Data;
using doan1.Models;
using doan1.Services;
using Microsoft.AspNetCore.Authorization;

namespace doan1.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : Controller
    {
        private readonly Data.HandmadeShopContext _context;
        private readonly IFileUploadService _fileUploadService;

        public UsersController(Data.HandmadeShopContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // Trang danh sách người dùng
        public async Task<IActionResult> Index(string searchTerm, string roleFilter, string sortBy = "id", string sortOrder = "asc")
        {
            try
            {
                // Sử dụng projection để tránh giá trị NULL
                var users = _context.Users.Select(u => new User
                {
                    Id = u.Id,
                    Username = u.Username ?? "",
                    Email = u.Email ?? "",
                    FullName = u.FullName ?? "",
                    Phone = u.Phone ?? "",
                    Address = u.Address ?? "",
                    Role = u.Role ?? "Customer",
                    ProfileImageUrl = u.ProfileImageUrl
                }).AsQueryable();

                // Tìm kiếm theo từ khóa
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    users = users.Where(u => 
                        (!string.IsNullOrEmpty(u.Username) && u.Username.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(u.FullName) && u.FullName.ToLower().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(u.Phone) && u.Phone.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(u.Address) && u.Address.ToLower().Contains(searchTerm))
                    );
                }

                // Lọc theo vai trò
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    users = users.Where(u => u.Role == roleFilter);
                }

                // Sắp xếp
                users = sortBy?.ToLower() switch
                {
                    "username" => sortOrder == "desc" ? users.OrderByDescending(u => u.Username) : users.OrderBy(u => u.Username),
                    "email" => sortOrder == "desc" ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email),
                    "fullname" => sortOrder == "desc" ? users.OrderByDescending(u => u.FullName) : users.OrderBy(u => u.FullName),
                    "role" => sortOrder == "desc" ? users.OrderByDescending(u => u.Role) : users.OrderBy(u => u.Role),
                    _ => sortOrder == "desc" ? users.OrderByDescending(u => u.Id) : users.OrderBy(u => u.Id)
                };

                // Truyền tham số về view để giữ lại giá trị tìm kiếm
                ViewBag.SearchTerm = searchTerm;
                ViewBag.RoleFilter = roleFilter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortOrder = sortOrder;

                var result = await users.ToListAsync();
                
                // Thông báo kết quả tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm) || !string.IsNullOrEmpty(roleFilter))
                {
                    ViewBag.SearchResults = $"Tìm thấy {result.Count} người dùng";
                }

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải dữ liệu người dùng: " + ex.Message;
                return View(new List<User>());
            }
        }

        // Trang chi tiết người dùng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var user = await _context.Users
                    .Select(u => new User
                    {
                        Id = u.Id,
                        Username = u.Username ?? "",
                        Email = u.Email ?? "",
                        FullName = u.FullName ?? "",
                        Phone = u.Phone ?? "",
                        Address = u.Address ?? "",
                        Role = u.Role ?? "Customer",
                        ProfileImageUrl = u.ProfileImageUrl
                    })
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin người dùng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Trang tạo mới người dùng
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý tạo mới người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,Password,Email,FullName,Address,Phone,Role")] User user, IFormFile? ProfileImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra username đã tồn tại chưa
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                        return View(user);
                    }

                    // Kiểm tra email đã tồn tại chưa
                    var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                    if (existingEmail != null)
                    {
                        ModelState.AddModelError("Email", "Email đã được sử dụng");
                        return View(user);
                    }

                    // Upload ảnh nếu có
                    if (ProfileImage != null && ProfileImage.Length > 0)
                    {
                        user.ProfileImageUrl = await _fileUploadService.UploadFileAsync(ProfileImage, "users");
                    }

                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm người dùng thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("ProfileImage", ex.Message);
                }
                catch (DbUpdateException dbEx)
                {
                    // Log chi tiết lỗi database
                    var innerException = dbEx.InnerException?.Message ?? dbEx.Message;
                    ModelState.AddModelError("", $"Lỗi database: {innerException}");
                    
                    // Kiểm tra các lỗi phổ biến
                    if (innerException.Contains("UNIQUE") || innerException.Contains("duplicate"))
                    {
                        ModelState.AddModelError("", "Tên đăng nhập hoặc email đã tồn tại");
                    }
                }
                catch (Exception ex)
                {
                    // Log chi tiết lỗi
                    var innerMessage = ex.InnerException?.Message ?? ex.Message;
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {innerMessage}");
                }
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var user = await _context.Users
                    .Select(u => new User
                    {
                        Id = u.Id,
                        Username = u.Username ?? "",
                        Password = u.Password ?? "",
                        Email = u.Email ?? "",
                        FullName = u.FullName ?? "",
                        Phone = u.Phone ?? "",
                        Address = u.Address ?? "",
                        Role = u.Role ?? "Customer",
                        ProfileImageUrl = u.ProfileImageUrl
                    })
                    .FirstOrDefaultAsync(u => u.Id == id);
                    
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin người dùng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Password,Email,FullName,Address,Phone,Role,ProfileImageUrl")] User user, 
            IFormFile? ProfileImage, bool ChangePassword = false)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            // Loại bỏ validation cho Username và Password vì chúng ta sẽ xử lý riêng
            ModelState.Remove("Username");
            if (!ChangePassword)
            {
                ModelState.Remove("Password");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy user hiện tại từ database
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    var oldImagePath = existingUser.ProfileImageUrl;

                    // Cập nhật các trường được phép thay đổi (không bao gồm Username)
                    existingUser.Email = user.Email;
                    existingUser.FullName = user.FullName;
                    existingUser.Address = user.Address;
                    existingUser.Phone = user.Phone;
                    existingUser.Role = user.Role;

                    // Chỉ cập nhật mật khẩu nếu có yêu cầu thay đổi
                    if (ChangePassword && !string.IsNullOrEmpty(user.Password))
                    {
                        existingUser.Password = user.Password;
                        TempData["Success"] = "Cập nhật người dùng và mật khẩu thành công!";
                    }
                    else
                    {
                        TempData["Success"] = "Cập nhật thông tin người dùng thành công!";
                    }

                    // Upload ảnh mới nếu có
                    if (ProfileImage != null && ProfileImage.Length > 0)
                    {
                        existingUser.ProfileImageUrl = await _fileUploadService.UploadFileAsync(ProfileImage, "users");
                        
                        // Xóa ảnh cũ
                        if (!string.IsNullOrEmpty(oldImagePath))
                        {
                            _fileUploadService.DeleteFile(oldImagePath);
                        }
                    }

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("ProfileImage", ex.Message);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật: " + ex.Message);
                }
            }
            
            // Nếu có lỗi, load lại user từ database để giữ nguyên dữ liệu hiện tại
            var userToReturn = await _context.Users.FindAsync(id);
            return View(userToReturn ?? user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var user = await _context.Users
                    .Select(u => new User
                    {
                        Id = u.Id,
                        Username = u.Username ?? "",
                        Email = u.Email ?? "",
                        FullName = u.FullName ?? "",
                        Phone = u.Phone ?? "",
                        Address = u.Address ?? "",
                        Role = u.Role ?? "Customer",
                        ProfileImageUrl = u.ProfileImageUrl
                    })
                    .FirstOrDefaultAsync(m => m.Id == id);
                    
                if (user == null)
                {
                    return NotFound();
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin người dùng: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa người dùng thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // Method để test và fix NULL data
        [HttpGet]
        public async Task<IActionResult> FixNullData()
        {
            try
            {
                var usersWithNullData = await _context.Users
                    .Where(u => u.Username == null || u.Email == null || u.Role == null)
                    .ToListAsync();

                foreach (var user in usersWithNullData)
                {
                    if (string.IsNullOrEmpty(user.Username))
                        user.Username = $"user_{user.Id}";
                    
                    if (string.IsNullOrEmpty(user.Email))
                        user.Email = $"user{user.Id}@example.com";
                    
                    if (string.IsNullOrEmpty(user.Role))
                        user.Role = "Customer";
                }

                if (usersWithNullData.Any())
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Đã sửa {usersWithNullData.Count} người dùng có dữ liệu NULL";
                }
                else
                {
                    TempData["Info"] = "Không có dữ liệu NULL nào cần sửa";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi sửa dữ liệu NULL: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
