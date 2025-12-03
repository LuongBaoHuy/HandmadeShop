using HandmadeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace HandmadeShop.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(HandmadeShopContext context, ILogger<CategoryController> logger) : base(context)
        {
            _logger = logger;
        }

        public ActionResult Category(int? id, int? page, string keyword, string sortBy = "name", 
            decimal? minPrice = null, decimal? maxPrice = null, int? minRating = null, 
            bool? isNew = null)
        {
            int pageSize = 12;
            int pageNumber = (page ?? 1);

            // Include Reviews để tính rating
            IQueryable<Product> query = db.Products.Include(p => p.Reviews);

            // Filter by keyword
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword));
                ViewBag.CategoryName = $"Kết quả tìm kiếm cho: {keyword}";
            }
            else if (id.HasValue)
            {
                var category = db.Categories.Find(id.Value);
                if (category == null)
                    return NotFound("Không tìm thấy danh mục với ID: " + id.Value);

                ViewBag.CategoryName = category.CategoriesName;
                query = query.Where(p => p.CategoryId == id.Value);
            }
            else
            {
                ViewBag.CategoryName = "Tất cả danh mục";
            }

            // Price filter
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Rating filter
            if (minRating.HasValue)
            {
                query = query.Where(p => p.Reviews.Any() && 
                    p.Reviews.Average(r => (double)r.Rating) >= minRating.Value);
            }

            // New products filter
            if (isNew.HasValue && isNew.Value)
            {
                query = query.Where(p => p.Stock > 10);
            }

            // Sorting
            switch (sortBy?.ToLower())
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "rating":
                    query = query.OrderByDescending(p => p.Reviews.Any() ? 
                        p.Reviews.Average(r => (double)r.Rating) : 0);
                    break;
                case "newest":
                    query = query.OrderByDescending(p => p.Stock).ThenByDescending(p => p.Id);
                    break;
                case "name":
                default:
                    query = query.OrderBy(p => p.Name);
                    break;
            }

            var products = query.ToPagedList(pageNumber, pageSize);

            if (products == null || !products.Any())
            {
                ViewBag.ErrorMessage = "Không có sản phẩm nào.";
                products = new List<Product>().ToPagedList(pageNumber, pageSize);
            }

            // Pass filter values to view
            ViewBag.CurrentSort = sortBy;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.MinRating = minRating;
            ViewBag.IsNew = isNew;
            ViewBag.Keyword = keyword;

            ViewBag.Categories = db.Categories.ToList();
            ViewBag.CategoryId = id;

            // Get price range for slider
            var allProducts = db.Products.Where(p => p.Price.HasValue);
            if (id.HasValue)
            {
                allProducts = allProducts.Where(p => p.CategoryId == id.Value);
            }
            
            ViewBag.MinPriceRange = allProducts.Any() ? allProducts.Min(p => p.Price ?? 0) : 0;
            ViewBag.MaxPriceRange = allProducts.Any() ? allProducts.Max(p => p.Price ?? 0) : 1000000;

            return View(products);
        }
    }
}
