using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkTeaShop.Data;

namespace MilkTeaShop.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context) => _context = context;

        public IActionResult Index()
        {
            var now   = DateTime.Now;
            var today = now.Date;
            var month = new DateTime(now.Year, now.Month, 1);

            var orders = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .ToList();

            ViewBag.DoanhThuHomNay  = orders.Where(o => o.OrderedAt.Date == today && o.PaymentStatus == "Paid").Sum(o => o.TotalAmount);
            ViewBag.DoanhThuThangNay = orders.Where(o => o.OrderedAt >= month && o.PaymentStatus == "Paid").Sum(o => o.TotalAmount);
            ViewBag.TongDoanhThu    = orders.Where(o => o.PaymentStatus == "Paid").Sum(o => o.TotalAmount);
            ViewBag.TongDon         = orders.Count;
            ViewBag.DonHomNay       = orders.Count(o => o.OrderedAt.Date == today);
            ViewBag.DonChoXuLy      = orders.Count(o => o.Status == "Pending");
            ViewBag.TongKhachHang   = _context.Customers.Count();
            ViewBag.TongSanPham     = _context.Products.Count();
            ViewBag.HetHang         = _context.Products.Count(p => !p.IsAvailable);

            // Doanh thu 7 ngày gần nhất
            var last7 = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .Select(d => new {
                    Ngay     = d.ToString("dd/MM"),
                    DoanhThu = orders.Where(o => o.OrderedAt.Date == d && o.PaymentStatus == "Paid").Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Ngay)
                .ToList();
            ViewBag.ChartLabels = string.Join(",", last7.Select(x => $"\"{x.Ngay}\""));
            ViewBag.ChartData   = string.Join(",", last7.Select(x => x.DoanhThu));

            // Top sản phẩm bán chạy
            ViewBag.TopProducts = _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new { Name = g.Key, Count = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // Đơn hàng gần nhất
            ViewBag.RecentOrders = orders.OrderByDescending(o => o.OrderedAt).Take(10).ToList();

            return View();
        }
    }
}
