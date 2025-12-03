using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using doan1.Data;
using doan1.Models;
using Microsoft.AspNetCore.Authorization;

namespace doan1.Controllers
{
    [Authorize(Policy = "AdminOrManager")]
    public class AttributesController : Controller
    {
        private readonly Data.HandmadeShopContext _context;
        public AttributesController(Data.HandmadeShopContext context)
        {
            _context = context;
        }

        // GET: Attributes
        public async Task<IActionResult> Index()
        {
            var attributes = await _context.Attributes.Include(a => a.AttributeOptions).ToListAsync();
            return View(attributes);
        }

        // GET: Attributes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Attributes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name")] doan1.Models.Attribute attribute)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attribute);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(attribute);
        }

        // GET: Attributes/Edit/5
    public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var attribute = await _context.Attributes.FindAsync(id);
            if (attribute == null) return NotFound();
            return View(attribute);
        }

        // POST: Attributes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] doan1.Models.Attribute attribute)
        {
            if (id != attribute.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(attribute);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(attribute);
        }

        // GET: Attributes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var attribute = await _context.Attributes.FindAsync(id);
            if (attribute == null) return NotFound();
            return View(attribute);
        }

        // POST: Attributes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var attribute = await _context.Attributes.FindAsync(id);
            if (attribute != null)
            {
                _context.Attributes.Remove(attribute);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Quản lý option cho từng thuộc tính
        public async Task<IActionResult> Options(int attributeId)
        {
            var attribute = await _context.Attributes.Include(a => a.AttributeOptions).FirstOrDefaultAsync(a => a.Id == attributeId);
            if (attribute == null) return NotFound();
            return View(attribute);
        }

        // POST: Thêm option cho thuộc tính
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOption(int attributeId, string value)
        {
            var attribute = await _context.Attributes.Include(a => a.AttributeOptions).FirstOrDefaultAsync(a => a.Id == attributeId);
            if (attribute == null) return NotFound();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var exists = attribute.AttributeOptions.Any(o => o.Value.ToLower() == value.ToLower());
                if (!exists)
                {
                    _context.AttributeOptions.Add(new AttributeOption { AttributeId = attributeId, Value = value });
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Options", new { attributeId });
        }

        // POST: Xóa option
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOption(int optionId, int attributeId)
        {
            var option = await _context.AttributeOptions.FindAsync(optionId);
            if (option != null)
            {
                _context.AttributeOptions.Remove(option);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Options", new { attributeId });
        }
    }
}
