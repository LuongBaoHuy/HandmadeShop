using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan1.Data;
using doan1.Models;
using Microsoft.AspNetCore.Authorization;

namespace doan1.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class CategoriesController : Controller
    {
        private readonly Data.HandmadeShopContext _context;

        public CategoriesController(Data.HandmadeShopContext context)
        {
            _context = context;
        }

        // Trang danh sách danh mục
        public async Task<IActionResult> Index(string searchTerm, string productCountFilter, string sortBy = "Name", string sortOrder = "asc")
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.ProductCountFilter = productCountFilter;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            var categoriesQuery = _context.Categories
                .Include(c => c.Products)
                .AsQueryable();

            // Tìm kiếm theo từ khóa (ID hoặc tên)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out int categoryId))
                {
                    categoriesQuery = categoriesQuery.Where(c => c.Id == categoryId || c.CategoriesName.Contains(searchTerm));
                }
                else
                {
                    categoriesQuery = categoriesQuery.Where(c => c.CategoriesName.Contains(searchTerm));
                }
            }

            // Lọc theo số lượng sản phẩm
            if (!string.IsNullOrEmpty(productCountFilter))
            {
                switch (productCountFilter)
                {
                    case "0":
                        categoriesQuery = categoriesQuery.Where(c => c.Products.Count() == 0);
                        break;
                    case "1-5":
                        categoriesQuery = categoriesQuery.Where(c => c.Products.Count() >= 1 && c.Products.Count() <= 5);
                        break;
                    case "6-10":
                        categoriesQuery = categoriesQuery.Where(c => c.Products.Count() >= 6 && c.Products.Count() <= 10);
                        break;
                    case "11+":
                        categoriesQuery = categoriesQuery.Where(c => c.Products.Count() >= 11);
                        break;
                }
            }

            // Sắp xếp danh mục
            switch (sortBy.ToLower())
            {
                case "id":
                    categoriesQuery = sortOrder == "desc" 
                        ? categoriesQuery.OrderByDescending(c => c.Id) 
                        : categoriesQuery.OrderBy(c => c.Id);
                    break;
                case "name":
                    categoriesQuery = sortOrder == "desc" 
                        ? categoriesQuery.OrderByDescending(c => c.CategoriesName) 
                        : categoriesQuery.OrderBy(c => c.CategoriesName);
                    break;
                case "productcount":
                    categoriesQuery = sortOrder == "desc" 
                        ? categoriesQuery.OrderByDescending(c => c.Products.Count()) 
                        : categoriesQuery.OrderBy(c => c.Products.Count());
                    break;
                default:
                    categoriesQuery = categoriesQuery.OrderBy(c => c.CategoriesName);
                    break;
            }

            var categories = await categoriesQuery.ToListAsync();
            ViewBag.TotalResults = categories.Count;
            
            return View(categories);
        }

        // Trang chi tiết danh mục
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // Trang tạo mới danh mục
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý tạo mới danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CategoriesName")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm danh mục thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Trang chỉnh sửa danh mục
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Xử lý cập nhật danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CategoriesName")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Trang xác nhận xóa danh mục
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra xem danh mục có sản phẩm không
            ViewBag.HasProducts = category.Products.Any();
            ViewBag.ProductCount = category.Products.Count();

            return View(category);
        }

        // Xử lý xóa danh mục
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (category != null)
                {
                    // Kiểm tra xem category có sản phẩm không
                    if (category.Products.Any())
                    {
                        TempData["Error"] = $"Không thể xóa danh mục này vì có {category.Products.Count()} sản phẩm liên quan. Vui lòng chuyển các sản phẩm sang danh mục khác trước.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa danh mục thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa danh mục: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
