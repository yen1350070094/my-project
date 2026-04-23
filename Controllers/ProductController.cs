using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkTeaShop.Data;
using MilkTeaShop.Models;

namespace MilkTeaShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context) => _context = context;

        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .OrderBy(p => p.Category != null ? p.Category.Name : "")
                .ThenBy(p => p.Name)
                .ToList();

            return View(products);
        }

        [Authorize(Roles = "Employee,Manager,Admin")]
        public IActionResult Manage()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Sizes)
                .OrderBy(p => p.Category != null ? p.Category.Name : "")
                .ThenBy(p => p.Name)
                .ToList();

            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(products);
        }

        [Authorize(Roles = "Employee,Manager,Admin")]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(new Product());
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee,Manager,Admin")]
        public IActionResult Create(Product product, decimal sizeS, decimal sizeM, decimal sizeL)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Sizes");
            ModelState.Remove("OrderItems");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
                return View(product);
            }

            _context.Products.Add(product);
            _context.SaveChanges();

            var productSizes = new List<ProductSize>
            {
                new ProductSize { ProductId = product.Id, SizeName = "S", ExtraPrice = sizeS },
                new ProductSize { ProductId = product.Id, SizeName = "M", ExtraPrice = sizeM },
                new ProductSize { ProductId = product.Id, SizeName = "L", ExtraPrice = sizeL }
            };

            _context.ProductSizes.AddRange(productSizes);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Manage");
        }

        [Authorize(Roles = "Employee,Manager,Admin")]
        public IActionResult Edit(int id)
        {
            var product = _context.Products.Include(p => p.Sizes).FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            return View(product);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee,Manager,Admin")]
        public IActionResult Edit(int id, Product product, decimal sizeS, decimal sizeM, decimal sizeL)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("Category");
            ModelState.Remove("Sizes");
            ModelState.Remove("OrderItems");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
                return View(product);
            }

            _context.Products.Update(product);

            var existingSizes = _context.ProductSizes.Where(s => s.ProductId == id).ToList();
            if (existingSizes != null)
            {
                _context.ProductSizes.RemoveRange(existingSizes);
            }

            var newSizes = new List<ProductSize>
            {
                new ProductSize { ProductId = id, SizeName = "S", ExtraPrice = sizeS },
                new ProductSize { ProductId = id, SizeName = "M", ExtraPrice = sizeM },
                new ProductSize { ProductId = id, SizeName = "L", ExtraPrice = sizeL }
            };

            _context.ProductSizes.AddRange(newSizes);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Manage");
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee,Manager,Admin")]
        public IActionResult ToggleAvailable(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            product.IsAvailable = !product.IsAvailable;
            _context.SaveChanges();

            return RedirectToAction("Manage");
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Include(p => p.Sizes).FirstOrDefault(p => p.Id == id);

            if (product != null)
            {
                if (product.Sizes != null)
                {
                    _context.ProductSizes.RemoveRange(product.Sizes);
                }

                _context.Products.Remove(product);
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Đã xóa sản phẩm!";
            return RedirectToAction("Manage");
        }
    }
}