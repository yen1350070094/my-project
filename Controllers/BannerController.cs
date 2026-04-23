using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkTeaShop.Data;
using MilkTeaShop.Models;

namespace MilkTeaShop.Controllers
{
    /// <summary>
    /// Admin / Manager / Employee: xem danh sách, thêm, sửa, xóa banner.
    /// Customer / khách: chỉ xem (qua trang chủ).
    /// </summary>
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class BannerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BannerController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ── Danh sách banner (trang quản lý) ──────────────────────────────
        public async Task<IActionResult> Index()
        {
            var banners = await _context.Banners
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(banners);
        }

        // ── Tạo mới ───────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Create() => View(new Banner());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner banner, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                banner.ImageUrl = await SaveImageAsync(imageFile);
            }

            if (!ModelState.IsValid) return View(banner);

            banner.CreatedAt = DateTime.Now;
            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm banner thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ── Chỉnh sửa ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Banner banner, IFormFile? imageFile)
        {
            if (id != banner.Id) return BadRequest();

            var existing = await _context.Banners.FindAsync(id);
            if (existing == null) return NotFound();

            // Upload ảnh mới nếu có
            if (imageFile != null && imageFile.Length > 0)
            {
                // Xóa ảnh cũ nếu là file local
                DeleteOldImage(existing.ImageUrl);
                banner.ImageUrl = await SaveImageAsync(imageFile);
            }
            else
            {
                banner.ImageUrl = existing.ImageUrl; // giữ ảnh cũ
            }

            if (!ModelState.IsValid) return View(banner);

            existing.Title        = banner.Title;
            existing.Description  = banner.Description;
            existing.ImageUrl     = banner.ImageUrl;
            existing.LinkUrl      = banner.LinkUrl;
            existing.DisplayOrder = banner.DisplayOrder;
            existing.IsActive     = banner.IsActive;
            existing.UpdatedAt    = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã cập nhật banner!";
            return RedirectToAction(nameof(Index));
        }

        // ── Xóa ───────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]   // Chỉ Admin & Manager được xóa
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();

            DeleteOldImage(banner.ImageUrl);
            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa banner!";
            return RedirectToAction(nameof(Index));
        }

        // ── Toggle Active / Inactive nhanh ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();

            banner.IsActive   = !banner.IsActive;
            banner.UpdatedAt  = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = banner.IsActive ? "Đã bật banner!" : "Đã tắt banner!";
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "images", "banners");
            Directory.CreateDirectory(uploadsDir);

            var ext      = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"banner_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/banners/{fileName}";
        }

        private void DeleteOldImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.StartsWith("http")) return;
            var fullPath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
