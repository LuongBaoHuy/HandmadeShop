using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using doan1.Data;
using doan1.Models;
using doan1.Services;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace doan1.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class ProductsController : Controller
    {
        private readonly Data.HandmadeShopContext _context;
        private readonly IFileUploadService _fileUploadService;

        public ProductsController(Data.HandmadeShopContext context, IFileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // Trang danh sách sản phẩm
        public async Task<IActionResult> Index(string searchTerm, int? categoryId, string stockFilter, string priceRange, string sortBy = "Name", string sortOrder = "asc")
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.StockFilter = stockFilter;
            ViewBag.PriceRange = priceRange;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            // Tải danh sách danh mục cho dropdown
            ViewBag.Categories = await _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.CategoriesName })
                .ToListAsync();

            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Tìm kiếm theo từ khóa (ID hoặc tên)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out int productId))
                {
                    productsQuery = productsQuery.Where(p => p.Id == productId || p.Name.Contains(searchTerm));
                }
                else
                {
                    productsQuery = productsQuery.Where(p => p.Name.Contains(searchTerm));
                }
            }

            // Lọc theo danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            // Lọc theo tồn kho
            if (!string.IsNullOrEmpty(stockFilter))
            {
                switch (stockFilter)
                {
                    case "in-stock":
                        productsQuery = productsQuery.Where(p => p.Stock.HasValue && p.Stock.Value > 0);
                        break;
                    case "out-of-stock":
                        productsQuery = productsQuery.Where(p => !p.Stock.HasValue || p.Stock.Value == 0);
                        break;
                    case "low-stock":
                        productsQuery = productsQuery.Where(p => p.Stock.HasValue && p.Stock.Value <= 10 && p.Stock.Value > 0);
                        break;
                }
            }

            // Lọc theo khoảng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "under-100k":
                        productsQuery = productsQuery.Where(p => p.Price.HasValue && p.Price.Value < 100000);
                        break;
                    case "100k-500k":
                        productsQuery = productsQuery.Where(p => p.Price.HasValue && p.Price.Value >= 100000 && p.Price.Value <= 500000);
                        break;
                    case "500k-1m":
                        productsQuery = productsQuery.Where(p => p.Price.HasValue && p.Price.Value > 500000 && p.Price.Value <= 1000000);
                        break;
                    case "over-1m":
                        productsQuery = productsQuery.Where(p => p.Price.HasValue && p.Price.Value > 1000000);
                        break;
                }
            }

            // Áp dụng sắp xếp
            switch (sortBy.ToLower())
            {
                case "id":
                    productsQuery = sortOrder == "desc" 
                        ? productsQuery.OrderByDescending(p => p.Id) 
                        : productsQuery.OrderBy(p => p.Id);
                    break;
                case "name":
                    productsQuery = sortOrder == "desc" 
                        ? productsQuery.OrderByDescending(p => p.Name) 
                        : productsQuery.OrderBy(p => p.Name);
                    break;
                case "price":
                    productsQuery = sortOrder == "desc" 
                        ? productsQuery.OrderByDescending(p => p.Price ?? 0) 
                        : productsQuery.OrderBy(p => p.Price ?? 0);
                    break;
                case "stock":
                    productsQuery = sortOrder == "desc" 
                        ? productsQuery.OrderByDescending(p => p.Stock ?? 0) 
                        : productsQuery.OrderBy(p => p.Stock ?? 0);
                    break;
                default:
                    productsQuery = productsQuery.OrderBy(p => p.Name);
                    break;
            }

            var products = await productsQuery.ToListAsync();
            ViewBag.TotalResults = products.Count;
            
            return View(products);
        }

        // Trang chi tiết sản phẩm
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Trang tạo mới sản phẩm
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoriesName");
            // Lấy danh sách thuộc tính và option
            var attributes = _context.Attributes
                .Include(a => a.AttributeOptions)
                .ToList();
            ViewBag.Attributes = attributes;
            var model = new CreateProductViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateProductViewModel model,
            List<IFormFile>? ProductImages,                   // <= nhiều ảnh
            [FromForm] List<int> SelectedAttributeOptionIds,
            [FromForm] int? MainImageIndex                    // <= index ảnh chính từ view
        )
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1) Tạo sản phẩm
                    decimal? normalizedDiscount = null;
                    if (model.DiscountedPrice.HasValue)
                    {
                        var v = model.DiscountedPrice.Value;
                        normalizedDiscount = v > 1m ? Math.Round(v / 100m, 4) : v;
                    }

                    var product = new Product
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        CategoryId = model.CategoryId,
                        Stock = model.Stock,
                        OriginalPrice = model.OriginalPrice,
                        DiscountedPrice = normalizedDiscount
                    };

                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    // 2) Upload nhiều ảnh -> set 1 ảnh chính
                    if (ProductImages != null && ProductImages.Count > 0)
                    {
                        var mainIdx = (MainImageIndex.HasValue && MainImageIndex.Value >= 0 && MainImageIndex.Value < ProductImages.Count)
                            ? MainImageIndex.Value
                            : 0;

                        for (int i = 0; i < ProductImages.Count; i++)
                        {
                            var file = ProductImages[i];
                            if (file == null || file.Length == 0) continue;

                            var url = await _fileUploadService.UploadFileAsync(file, "Products");

                            var isMain = (i == mainIdx);
                            // cập nhật cột ImageUrl của bảng Products cho ảnh chính
                            if (isMain)
                            {
                                product.ImageUrl = url;
                                _context.Update(product);
                            }

                            _context.Set<ProductImage>().Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = url,
                                IsMain = isMain
                            });
                        }

                        await _context.SaveChangesAsync();
                    }

                    // 3) (giữ nguyên) xử lý variations + thuộc tính...
                    if (model.Variations != null && model.Variations.Any())
                    {
                        await CreateProductVariations(product.Id, model.Variations);
                        await RecalculateProductAggregates(product.Id);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        return RedirectToAction("AddVariants", new { id = product.Id });
                    }

                    // 4) Fallback: nếu không có Variations từ Shopee UI, mới xử lý theo luồng chọn sẵn option (cũ)
                    var allOptionIds = SelectedAttributeOptionIds != null
                        ? new List<int>(SelectedAttributeOptionIds)
                        : new List<int>();

                    // Nhặt thêm các NewOption_{attrId} (nếu có)
                    foreach (var key in Request.Form.Keys)
                    {
                        if (!key.StartsWith("NewOption_")) continue;

                        var attrIdStr = key.Substring("NewOption_".Length);
                        if (!int.TryParse(attrIdStr, out int attrId)) continue;

                        var newValue = (Request.Form[key].ToString() ?? string.Empty).Trim();
                        if (string.IsNullOrEmpty(newValue)) continue;

                        var exists = await _context.AttributeOptions
                            .AnyAsync(o => o.AttributeId == attrId && o.Value.ToLower() == newValue.ToLower());

                        if (!exists)
                        {
                            var newOpt = new AttributeOption { AttributeId = attrId, Value = newValue };
                            _context.AttributeOptions.Add(newOpt);
                            await _context.SaveChangesAsync();
                            allOptionIds.Add(newOpt.Id);
                        }
                        else
                        {
                            var existOpt = await _context.AttributeOptions
                                .FirstOrDefaultAsync(o => o.AttributeId == attrId && o.Value.ToLower() == newValue.ToLower());
                            if (existOpt != null) allOptionIds.Add(existOpt.Id);
                        }
                    }

                    // Liên kết ProductAttributeOptions + tự sinh biến thể từ các option đã chọn
                    if (allOptionIds.Any())
                    {
                        foreach (var optionId in allOptionIds.Distinct())
                        {
                            _context.ProductAttributeOptions.Add(new ProductAttributeOption
                            {
                                ProductId = product.Id,
                                AttributeOptionId = optionId
                            });
                        }
                        await _context.SaveChangesAsync();

                        var optionList = await _context.AttributeOptions
                            .Where(o => allOptionIds.Contains(o.Id))
                            .Include(o => o.Attribute)
                            .ToListAsync();

                        var grouped = optionList.GroupBy(o => o.AttributeId).ToList();
                        var allCombinations = GenerateCombinations(grouped.Select(g => g.ToList()).ToList());

                        foreach (var combo in allCombinations)
                        {
                            // combo là List<int> (các optionId)
                            var optionIds = combo.ToList();
                            var hash = await BuildStableCombinationHash(product.Id, optionIds);

                            var exists = await _context.ProductVariations
                                .AnyAsync(pv => pv.ProductId == product.Id && pv.CombinationHash == hash);
                            if (exists) continue;

                            var variation = new ProductVariation
                            {
                                ProductId = product.Id,
                                Price = model.Price ?? 0,
                                Stock = model.Stock ?? 0,
                                CombinationHash = hash
                            };
                            _context.ProductVariations.Add(variation);
                            await _context.SaveChangesAsync();

                            foreach (var opt in optionIds)
                            {
                                _context.VariationOptionLinks.Add(new VariationOptionLink
                                {
                                    VariationId = variation.Id,
                                    AttributeOptionId = opt
                                });
                            }
                        }

                        await _context.SaveChangesAsync();
                        await RecalculateProductAggregates(product.Id);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                    return RedirectToAction("AddVariants", new { id = product.Id });
                }
                catch (InvalidOperationException ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("ProductImage", ex.Message);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var errorMessage = "Có lỗi xảy ra khi tạo sản phẩm: " + ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMessage += " Chi tiết: " + ex.InnerException.Message;
                        if (ex.InnerException.InnerException != null)
                        {
                            errorMessage += " Root cause: " + ex.InnerException.InnerException.Message;
                        }
                    }
                    Console.WriteLine($"Error creating product: {ex}");
                    ModelState.AddModelError("", errorMessage);
                }
            }
            else
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"ModelState Error - {state.Key}: {error.ErrorMessage}");
                    }
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoriesName", model.CategoryId);
            ViewBag.Attributes = _context.Attributes.Include(a => a.AttributeOptions).ToList();
            return View(model);
        }

        // Trang chỉnh sửa sản phẩm
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoriesName", product.CategoryId);
            return View(product);
        }

        // Xử lý cập nhật sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,CategoryId,Stock,OriginalPrice,DiscountedPrice,ImageUrl")] Product product, IFormFile? ProductImage)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Chuẩn hóa DiscountedPrice nhập theo %
                    if (product.DiscountedPrice.HasValue)
                    {
                        var v = product.DiscountedPrice.Value;
                        product.DiscountedPrice = v > 1m ? Math.Round(v / 100m, 4) : v;
                    }

                    // Lấy sản phẩm cũ để có thể xóa ảnh cũ nếu cần
                    var oldProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    var oldImagePath = oldProduct?.ImageUrl;

                    // Tải lên ảnh mới nếu có
                    if (ProductImage != null && ProductImage.Length > 0)
                    {
                        var newUrl = await _fileUploadService.UploadFileAsync(ProductImage, "Products");

                        // Xóa file ảnh cũ (nếu có)
                        if (!string.IsNullOrEmpty(oldImagePath))
                        {
                            _fileUploadService.DeleteFile(oldImagePath);
                        }

                        // Cập nhật URL ảnh trên bảng Products (giữ tương thích)
                        product.ImageUrl = newUrl;

                        // Đồng bộ ProductImages (IsMain = true)
                        var mainImage = await _context.Set<ProductImage>()
                            .FirstOrDefaultAsync(pi => pi.ProductId == product.Id && pi.IsMain);
                        if (mainImage == null)
                        {
                            _context.Set<ProductImage>().Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImageUrl = newUrl,
                                IsMain = true
                            });
                        }
                        else
                        {
                            mainImage.ImageUrl = newUrl;
                            mainImage.IsMain = true;
                            _context.Set<ProductImage>().Update(mainImage);
                        }
                    }
                    else
                    {
                        // Giữ nguyên ảnh cũ nếu không tải lên ảnh mới
                        product.ImageUrl = oldImagePath;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("ProductImage", ex.Message);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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
                    var errorMessage = "Có lỗi xảy ra khi cập nhật: " + ex.Message;
                    if (ex.InnerException != null)
                    {
                        errorMessage += " Chi tiết: " + ex.InnerException.Message;
                    }
                    ModelState.AddModelError("", errorMessage);
                }
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "CategoriesName", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                // 1) Load related rows
                var images = await _context.Set<ProductImage>()
                    .Where(pi => pi.ProductId == id)
                    .ToListAsync();

                var variations = await _context.ProductVariations
                    .Include(v => v.VariationOptionLinks)
                    .Where(v => v.ProductId == id)
                    .ToListAsync();

                var variationLinks = variations.SelectMany(v => v.VariationOptionLinks).ToList();

                var productAttributeOptions = await _context.ProductAttributeOptions
                    .Where(pao => pao.ProductId == id)
                    .ToListAsync();

                // 2) Delete files from storage (ignore failures)
                try
                {
                    foreach (var img in images)
                    {
                        if (!string.IsNullOrWhiteSpace(img.ImageUrl))
                            _fileUploadService.DeleteFile(img.ImageUrl);
                    }

                    if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                    {
                        // In case main image is not in ProductImages table or duplicated, delete once more safely
                        if (!images.Any(i => string.Equals(i.ImageUrl, product.ImageUrl, StringComparison.OrdinalIgnoreCase)))
                        {
                            _fileUploadService.DeleteFile(product.ImageUrl);
                        }
                    }
                }
                catch
                {
                    // Swallow file delete errors so DB deletion can continue
                }

                // 3) Delete related rows (respect FK order)
                if (variationLinks.Count > 0)
                    _context.VariationOptionLinks.RemoveRange(variationLinks);

                if (variations.Count > 0)
                    _context.ProductVariations.RemoveRange(variations);

                if (productAttributeOptions.Count > 0)
                    _context.ProductAttributeOptions.RemoveRange(productAttributeOptions);

                if (images.Count > 0)
                    _context.Set<ProductImage>().RemoveRange(images);

                // 4) Delete product
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["Success"] = "Xóa sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine($"Delete product {id} failed: {ex}");
                TempData["Error"] = "Không thể xóa sản phẩm vì đang có dữ liệu liên quan (đơn hàng, giỏ hàng hoặc tham chiếu khác). Vui lòng xóa tham chiếu trước rồi thử lại.";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Console.WriteLine($"Delete product {id} unexpected error: {ex}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa sản phẩm. Vui lòng thử lại sau.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // API để lấy danh sách thuộc tính cho sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetProductAttributes(int productId)
        {
            var attributes = await _context.ProductAttributeOptions
                .Where(pao => pao.ProductId == productId)
                .Include(pao => pao.AttributeOption)
                .ThenInclude(ao => ao.Attribute)
                .GroupBy(pao => pao.AttributeOption.Attribute)
                .Select(g => new ProductAttributeViewModel
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Options = g.Select(pao => pao.AttributeOption.Value).ToList()
                })
                .ToListAsync();

            return Json(attributes);
        }

        // API để lấy danh sách biến thể của sản phẩm
        [HttpGet]
        public async Task<IActionResult> GetProductVariations(int productId)
        {
            var variations = await _context.ProductVariations
                .Where(pv => pv.ProductId == productId)
                .Include(pv => pv.VariationOptionLinks)
                .ThenInclude(vol => vol.AttributeOption)
                .ThenInclude(ao => ao.Attribute)
                .Select(pv => new ProductVariationViewModel
                {
                    Id = pv.Id,
                    ProductId = pv.ProductId,
                    Price = pv.Price,
                    Stock = pv.Stock,
                    CombinationHash = pv.CombinationHash ?? "",
                    Attributes = pv.VariationOptionLinks.ToDictionary(
                        vol => vol.AttributeOption.Attribute.Name,
                        vol => vol.AttributeOption.Value
                    ),
                    DisplayName = string.Join(", ", pv.VariationOptionLinks
                        .Select(vol => $"{vol.AttributeOption.Attribute.Name}: {vol.AttributeOption.Value}"))
                })
                .ToListAsync();

            return Json(variations);
        }

        // Tạo biến thể cho sản phẩm
        [HttpPost]
    public async Task<IActionResult> GenerateVariants(int productId)
        {
            try
            {
                // Lấy tất cả thuộc tính của sản phẩm
                var productAttributes = await _context.ProductAttributeOptions
                    .Where(pao => pao.ProductId == productId)
                    .Include(pao => pao.AttributeOption)
                    .ThenInclude(ao => ao.Attribute)
                    .GroupBy(pao => pao.AttributeOption.Attribute)
                    .ToListAsync();

                if (!productAttributes.Any())
                {
                    return Json(new { success = false, message = "Sản phẩm chưa có thuộc tính nào!" });
                }

                // Tạo tất cả tổ hợp có thể
                var optionGroups = productAttributes
                    .Select(g => g.Select(pao => pao.AttributeOption).ToList())
                    .ToList();
                var combinations = GenerateCombinations(optionGroups);

                // Lấy giá cơ bản của sản phẩm
                var product = await _context.Products.FindAsync(productId);
                var basePrice = product?.Price ?? 0;

                foreach (var combination in combinations)
                {
                    // combination là List<int> các optionId
                    var hash = await BuildStableCombinationHash(productId, combination);

                    // Kiểm tra biến thể đã tồn tại chưa
                    var existingVariation = await _context.ProductVariations
                        .FirstOrDefaultAsync(pv => pv.ProductId == productId && pv.CombinationHash == hash);

                    if (existingVariation == null)
                    {
                        // Tạo biến thể mới
                        var variation = new ProductVariation
                        {
                            ProductId = productId,
                            Price = basePrice,
                            Stock = 0,
                            CombinationHash = hash
                        };

                        _context.ProductVariations.Add(variation);
                        await _context.SaveChangesAsync();

                        // Tạo liên kết với các tùy chọn thuộc tính
                        foreach (var optionId in combination)
                        {
                            var link = new VariationOptionLink
                            {
                                VariationId = variation.Id,
                                AttributeOptionId = optionId
                            };
                            _context.VariationOptionLinks.Add(link);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                // Sau khi sinh biến thể hàng loạt, cập nhật lại giá & tồn kho tổng
                await RecalculateProductAggregates(productId);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Tạo biến thể thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Cập nhật thông tin biến thể
        [HttpPost]
        public async Task<IActionResult> UpdateVariation([FromBody] UpdateVariationRequest request)
        {
            try
            {
                var variation = await _context.ProductVariations.FindAsync(request.VariationId);
                if (variation == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy biến thể!" });
                }

                variation.Price = request.Price;
                variation.Stock = request.Stock;
                await _context.SaveChangesAsync();
                await RecalculateProductAggregates(variation.ProductId);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật biến thể thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Xóa biến thể
        [HttpPost]
        public async Task<IActionResult> DeleteVariation(int variationId)
        {
            try
            {
                var variation = await _context.ProductVariations
                    .Include(pv => pv.VariationOptionLinks)
                    .FirstOrDefaultAsync(pv => pv.Id == variationId);

                if (variation == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy biến thể!" });
                }

                _context.VariationOptionLinks.RemoveRange(variation.VariationOptionLinks);
                _context.ProductVariations.Remove(variation);
                await _context.SaveChangesAsync();
                await RecalculateProductAggregates(variation.ProductId);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa biến thể thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Tạo biến thể từ danh sách Variations gửi kèm form (UI kiểu Shopee)
        private async Task CreateProductVariations(int productId, List<CreateVariationViewModel> variations)
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var v in variations)
            {
                if (string.IsNullOrWhiteSpace(v.Attributes)) continue;

                var attrs = JsonSerializer.Deserialize<List<VariationAttributeViewModel>>(v.Attributes, jsonOptions);
                if (attrs == null || attrs.Count == 0) continue;

                var optionIds = new List<int>();

                foreach (var a in attrs)
                {
                    var attrName = (a.Name ?? "").Trim();
                    var optValue = (a.Value ?? "").Trim();
                    if (attrName.Length == 0 || optValue.Length == 0) continue;

                    // 1) Tìm hoặc tạo Attribute
                    var attribute = await _context.Attributes
                        .FirstOrDefaultAsync(x => x.Name.ToLower() == attrName.ToLower());
                    if (attribute == null)
                    {
                        attribute = new Models.Attribute { Name = attrName };
                        _context.Attributes.Add(attribute);
                        await _context.SaveChangesAsync();
                    }

                    // 2) Tìm hoặc tạo AttributeOption
                    var option = await _context.AttributeOptions
                        .FirstOrDefaultAsync(x => x.AttributeId == attribute.Id && x.Value.ToLower() == optValue.ToLower());
                    if (option == null)
                    {
                        option = new AttributeOption { AttributeId = attribute.Id, Value = optValue };
                        _context.AttributeOptions.Add(option);
                        await _context.SaveChangesAsync();
                    }

                    optionIds.Add(option.Id);

                    // 3) Liên kết ProductAttributeOptions
                    var hasLink = await _context.ProductAttributeOptions
                        .AnyAsync(p => p.ProductId == productId && p.AttributeOptionId == option.Id);
                    if (!hasLink)
                    {
                        _context.ProductAttributeOptions.Add(new ProductAttributeOption
                        {
                            ProductId = productId,
                            AttributeOptionId = option.Id
                        });
                    }
                }

                if (!optionIds.Any()) continue;

                // 4) Hash kết hợp và tạo/cập nhật biến thể
                var hash = await BuildStableCombinationHash(productId, optionIds);
                var existing = await _context.ProductVariations
                    .Include(pv => pv.VariationOptionLinks)
                    .FirstOrDefaultAsync(pv => pv.ProductId == productId && pv.CombinationHash == hash);

                if (existing == null)
                {
                    var variation = new ProductVariation
                    {
                        ProductId = productId,
                        Price = v.Price,
                        Stock = v.Stock,
                        CombinationHash = hash
                    };
                    _context.ProductVariations.Add(variation);
                    await _context.SaveChangesAsync();

                    foreach (var oid in optionIds)
                    {
                        _context.VariationOptionLinks.Add(new VariationOptionLink
                        {
                            VariationId = variation.Id,
                            AttributeOptionId = oid
                        });
                    }
                }
                else
                {
                    // cập nhật giá/stock và đồng bộ links
                    existing.Price = v.Price;
                    existing.Stock = v.Stock;

                    var currentIds = existing.VariationOptionLinks.Select(l => l.AttributeOptionId).ToHashSet();
                    var targetIds = optionIds.ToHashSet();

                    // thêm missing
                    foreach (var addId in targetIds.Except(currentIds))
                    {
                        _context.VariationOptionLinks.Add(new VariationOptionLink
                        {
                            VariationId = existing.Id,
                            AttributeOptionId = addId
                        });
                    }
                    // xóa dư (hiếm khi xảy ra)
                    foreach (var del in existing.VariationOptionLinks.Where(l => !targetIds.Contains(l.AttributeOptionId)).ToList())
                    {
                        _context.VariationOptionLinks.Remove(del);
                    }
                }
            }
        }

        // Tính lại tổng tồn kho và cập nhật giá đại diện của Product
        private async Task RecalculateProductAggregates(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return;

            var variations = await _context.ProductVariations
                .Where(pv => pv.ProductId == productId)
                .ToListAsync();

            if (variations.Any())
            {
                product.Stock = variations.Sum(v => v.Stock);
                // Lấy giá nhỏ nhất trong các biến thể làm giá đại diện
                var minPrice = variations.Min(v => v.Price);
                product.Price = (product.Price.HasValue && product.Price.Value > 0)
                    ? Math.Min(product.Price.Value, minPrice)
                    : minPrice;
            }
            else
            {
                // Không có biến thể -> giữ nguyên giá/stock đã nhập
            }

            await _context.SaveChangesAsync();
        }

        // Sinh tổ hợp từ các nhóm option (List<List<AttributeOption>>) -> List các List<int> (Id)
        private List<List<int>> GenerateCombinations(List<List<AttributeOption>> optionGroups)
        {
            var results = new List<List<int>>();
            if (optionGroups == null || optionGroups.Count == 0) return results;

            void Recurse(int idx, List<int> current)
            {
                if (idx == optionGroups.Count)
                {
                    results.Add(new List<int>(current));
                    return;
                }
                foreach (var opt in optionGroups[idx])
                {
                    current.Add(opt.Id);
                    Recurse(idx + 1, current);
                    current.RemoveAt(current.Count - 1);
                }
            }

            Recurse(0, new List<int>());
            return results;
        }

        // Hash kết hợp (string) theo dạng "TênThuộcTính:GiáTrị|TênThuộcTính2:GiáTrị2"
        // Quy tắc UNIQUE toàn bảng:
        // - Nếu baseHash đã dùng bởi cùng product -> dùng lại
        // - Nếu baseHash đã dùng bởi product khác -> prefix "{productId}:" để tránh trùng
        // - Nếu chưa ai dùng -> dùng baseHash
        private async Task<string> BuildStableCombinationHash(int productId, List<int> optionIds)
        {
            // Lấy tên thuộc tính và giá trị từ danh sách optionIds
            var options = await _context.AttributeOptions
                .Where(o => optionIds.Contains(o.Id))
                .Include(o => o.Attribute)
                .ToListAsync();

            // Sắp xếp ổn định để chuỗi nhất quán (theo tên thuộc tính rồi tên giá trị)
            var parts = options
                .Where(o => o.Attribute != null)
                .OrderBy(o => o.Attribute!.Name)
                .ThenBy(o => o.Value)
                .Select(o => $"{o.Attribute!.Name}:{o.Value}")
                .ToList();

            // Fallback nếu vì lý do nào đó không lấy được đủ dữ liệu
            var baseHash = parts.Count > 0  
                ? string.Join("|", parts)
                : string.Join("-", optionIds.OrderBy(i => i));

            // Đã tồn tại cùng sản phẩm -> dùng lại
            bool usedBySame = await _context.ProductVariations
                .AnyAsync(pv => pv.ProductId == productId && pv.CombinationHash == baseHash);
            if (usedBySame) return baseHash;

            // Dùng bởi sản phẩm khác -> prefix productId để tránh xung đột UNIQUE toàn bảng
            bool usedByOther = await _context.ProductVariations
                .AnyAsync(pv => pv.ProductId != productId && pv.CombinationHash == baseHash);
            if (usedByOther)
            {
                return $"{productId}:{baseHash}";
            }

            return baseHash;
        }

        // API endpoints for order creation
        [HttpGet]
        [Route("api/products/{id}/price")]
        public async Task<IActionResult> GetProductPrice(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Json(new { price = product.Price ?? 0 });
        }

        [HttpGet]
        [Route("api/products/{id}/variations")]
        public async Task<IActionResult> GetProductVariationsApi(int id)
        {
            var variations = await _context.ProductVariations
                .Include(pv => pv.VariationOptionLinks)
                    .ThenInclude(vol => vol.AttributeOption)
                        .ThenInclude(ao => ao.Attribute)
                .Where(pv => pv.ProductId == id)
                .ToListAsync();

            var result = variations.Select(v => new
            {
                id = v.Id,
                price = v.Price,
                stock = v.Stock,
                attributes = v.VariationOptionLinks.Select(vol => new
                {
                    attributeName = vol.AttributeOption.Attribute.Name,
                    optionValue = vol.AttributeOption.Value
                }).ToList()
            }).ToList();

            return Json(result);
        }

        // Bước 2: Hiển thị giao diện thêm biến thể cho sản phẩm
        [HttpGet]
        public async Task<IActionResult> AddVariants(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            // Có thể load thêm ViewBag.Attributes nếu cần
            return View(product);
        }

        // Bước 2: Xử lý lưu biến thể (cơ bản)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddVariants(int id, IFormCollection form)
        {
            return RedirectToAction("Details", new { id });
        }
    }
}
