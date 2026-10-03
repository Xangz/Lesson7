using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShopMvc.Data;
using ShopMvc.Models;
using ShopMvc.ViewModels;

namespace ShopMvc.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _db.Products.Include(p => p.Category).OrderByDescending(p => p.Id).ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: Products/Create
        public async Task<IActionResult> Create()
        {
            await LoadCategories();
            return View(new ProductViewModel());
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel vm)
        {
            await ValidateCategory(vm);
            if (!ModelState.IsValid)
            {
                await LoadCategories(vm.CategoryId);
                return View(vm);
            }

            var product = new Product
            {
                Name = vm.Name.Trim(),
                Price = vm.Price.Value,
                SalePrice = vm.SalePrice.Value,
                Description = vm.Description.Trim(),
                CategoryId = vm.CategoryId.Value,
                Image = await SaveImage(vm.ImageFile)
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Thêm sản phẩm thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();

            var vm = new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Image = p.Image,
                Price = p.Price,
                SalePrice = p.SalePrice,
                Description = p.Description,
                CategoryId = p.CategoryId
            };
            await LoadCategories(vm.CategoryId);
            return View(vm);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Giữ ảnh cũ để validate "bắt buộc có ảnh"
            vm.Image = product.Image;

            await ValidateCategory(vm);
            if (!ModelState.IsValid)
            {
                await LoadCategories(vm.CategoryId);
                return View(vm);
            }

            product.Name = vm.Name.Trim();
            product.Price = vm.Price.Value;
            product.SalePrice = vm.SalePrice.Value;
            product.Description = vm.Description.Trim();
            product.CategoryId = vm.CategoryId.Value;

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                DeleteImage(product.Image);
                product.Image = await SaveImage(vm.ImageFile);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật sản phẩm thành công";
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                DeleteImage(product.Image);
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Xóa sản phẩm thành công";
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------- Helpers ----------

        private async Task LoadCategories(int? selectedId = null)
        {
            var categories = await _db.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
        }

        // CategoryId phải nằm trong danh sách Category
        private async Task ValidateCategory(ProductViewModel vm)
        {
            if (vm.CategoryId.HasValue && !await _db.Categories.AnyAsync(c => c.Id == vm.CategoryId.Value))
                ModelState.AddModelError(nameof(vm.CategoryId), "Danh mục không tồn tại trong danh sách");
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, "products");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
            using var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
            return fileName;
        }

        private void DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var path = Path.Combine(_env.WebRootPath, "products", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
    }
}
