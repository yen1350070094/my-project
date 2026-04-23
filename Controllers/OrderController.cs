using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkTeaShop.Data;
using MilkTeaShop.Models;
using MilkTeaShop.ViewModels;

namespace MilkTeaShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        // ══════════════════════════════════════════════
        //  CẤU HÌNH VIETQR — chỉ cần đổi 3 dòng này
        // ══════════════════════════════════════════════
        private const string BankId = "BIDV";           
        private const string AccountNo = "8870184518";   
        private const string AccountName = "MILKTEASHOP";  

        public OrderController(ApplicationDbContext context) => _context = context;

        // ─────────────────────────────────────────────
        //  Helper: tạo URL ảnh QR VietQR
        // ─────────────────────────────────────────────
        private static string BuildVietQrUrl(long amount, string orderCode)
        {
            var memo = Uri.EscapeDataString($"Thanh toan {orderCode}");
            return $"https://img.vietqr.io/image/{BankId}-{AccountNo}-compact2.png" +
                   $"?amount={amount}&addInfo={memo}&accountName={AccountName}";
        }

        // ─────────────────────────────────────────────
        //  STAFF: Danh sách tất cả đơn
        // ─────────────────────────────────────────────
        [Authorize(Roles = "Employee,Manager,Admin")]
        public IActionResult Index()
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.ProductSize)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.SugarLevel)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.IceLevel)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.OrderItemToppings).ThenInclude(t => t.Topping)
                .OrderByDescending(o => o.OrderedAt)
                .ToList();
            return View(orders);
        }

        // ─────────────────────────────────────────────
        //  CUSTOMER: Lịch sử đơn hàng
        // ─────────────────────────────────────────────
        [Authorize(Roles = "Customer")]
        public IActionResult MyOrders()
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);
            if (customer == null) return RedirectToAction("Index", "Home");

            var orders = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.ProductSize)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.SugarLevel)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.IceLevel)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.OrderItemToppings).ThenInclude(t => t.Topping)
                .Where(o => o.CustomerId == customer.Id)
                .OrderByDescending(o => o.OrderedAt)
                .ToList();
            return View(orders);
        }

        // ─────────────────────────────────────────────
        //  CUSTOMER: Theo dõi đơn hàng
        // ─────────────────────────────────────────────
        [Authorize(Roles = "Customer")]
        public IActionResult Track(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);

            var order = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.ProductSize)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.SugarLevel)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.IceLevel)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.OrderItemToppings).ThenInclude(t => t.Topping)
                .FirstOrDefault(o => o.Id == id && o.CustomerId == customer!.Id);

            if (order == null) return NotFound();
            return View(order);
        }

        // ─────────────────────────────────────────────
        //  CUSTOMER: Hủy đơn
        // ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public IActionResult Cancel(int id, string? reason)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);

            var order = _context.Orders.FirstOrDefault(o => o.Id == id && o.CustomerId == customer!.Id);
            if (order == null) return NotFound();

            if (order.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn đang được xử lý hoặc đã hoàn thành!";
                return RedirectToAction("Track", new { id });
            }

            order.Status = "Cancelled";
            order.CancelReason = reason ?? "Khách hàng tự hủy";
            order.CancelledAt = DateTime.Now;

            // Hoàn tiền nếu Momo/VietQR và đã thanh toán
            if (order.PaymentMethod == "Momo" && order.PaymentStatus == "Paid")
            {
                order.PaymentStatus = "Refunded";
                TempData["SuccessMessage"] = $"Đã hủy đơn {order.OrderCode}. Tiền sẽ được hoàn lại trong 1-3 ngày làm việc!";
            }
            else
            {
                TempData["SuccessMessage"] = $"Đã hủy đơn {order.OrderCode} thành công!";
            }

            _context.SaveChanges();
            return RedirectToAction("MyOrders");
        }

        // ─────────────────────────────────────────────
        //  CUSTOMER: GET Đặt hàng
        // ─────────────────────────────────────────────
        [Authorize(Roles = "Customer")]
        public IActionResult Create(int productId)
        {
            var product = _context.Products.Include(p => p.Sizes).FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound();
            if (!product.IsAvailable)
            {
                TempData["ErrorMessage"] = "Sản phẩm này hiện đã hết hàng!";
                return RedirectToAction("Index", "Home");
            }

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);

            var vm = new CreateOrderViewModel
            {
                ProductId = productId,
                Product = product,
                Sizes = product.Sizes.ToList(),
                Toppings = _context.Toppings.Where(t => t.IsAvailable).ToList(),
                SugarLevels = _context.SugarLevels.OrderBy(s => s.Percentage).ToList(),
                IceLevels = _context.IceLevels.OrderBy(i => i.Percentage).ToList(),
                CustomerName = customer?.FullName ?? "",
                CustomerPhone = customer?.Phone ?? "",
                CustomerAddress = customer?.Address ?? "",
            };
            return View(vm);
        }

        // ─────────────────────────────────────────────
        //  CUSTOMER: POST Đặt hàng
        // ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public IActionResult Create(CreateOrderViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var product = _context.Products.Include(p => p.Sizes).FirstOrDefault(p => p.Id == vm.ProductId);
                if (product == null) return NotFound();
                vm.Product = product;
                vm.Sizes = product.Sizes.ToList();
                vm.Toppings = _context.Toppings.Where(t => t.IsAvailable).ToList();
                vm.SugarLevels = _context.SugarLevels.OrderBy(s => s.Percentage).ToList();
                vm.IceLevels = _context.IceLevels.OrderBy(i => i.Percentage).ToList();
                return View(vm);
            }

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var customer = _context.Customers.FirstOrDefault(c => c.UserId == userId);
            if (customer == null) return Unauthorized();

            customer.Address = vm.CustomerAddress;
            _context.SaveChanges();

            var size = _context.ProductSizes.Find(vm.SelectedSizeId);
            var prod = _context.Products.Find(vm.ProductId);
            var toppings = _context.Toppings.Where(t => vm.SelectedToppingIds.Contains(t.Id)).ToList();
            var unitPrice = (prod?.BasePrice ?? 0) + (size?.ExtraPrice ?? 0);
            var subtotal = (unitPrice + toppings.Sum(t => t.Price)) * vm.Quantity;

            // Momo/VietQR → Unpaid, chờ nhân viên xác nhận chuyển khoản
            // Cash        → Unpaid, chờ nhân viên xác nhận tiền mặt
            var order = new Order
            {
                CustomerId = customer.Id,
                OrderCode = $"MTS-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                TotalAmount = subtotal,
                Status = "Pending",
                PaymentMethod = vm.PaymentMethod,
                PaymentStatus = "Unpaid",
                Note = vm.Note,
                OrderedAt = DateTime.Now
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            var item = new OrderItem
            {
                OrderId = order.Id,
                ProductId = vm.ProductId,
                ProductSizeId = vm.SelectedSizeId,
                SugarLevelId = vm.SelectedSugarLevelId,
                IceLevelId = vm.SelectedIceLevelId,
                Quantity = vm.Quantity,
                UnitPrice = unitPrice,
                Subtotal = subtotal,
                Note = vm.Note
            };
            _context.OrderItems.Add(item);
            _context.SaveChanges();

            foreach (var t in toppings)
                _context.OrderItemToppings.Add(new OrderItemTopping
                {
                    OrderItemId = item.Id,
                    ToppingId = t.Id,
                    Price = t.Price
                });
            _context.SaveChanges();

            return RedirectToAction("Payment", new { id = order.Id });
        }

        // ─────────────────────────────────────────────
        //  GET: Trang thanh toán (có truyền QR URL cho VietQR)
        // ─────────────────────────────────────────────
        public IActionResult Payment(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.ProductSize)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.SugarLevel)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.IceLevel)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.OrderItemToppings).ThenInclude(t => t.Topping)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();

            var vm = new PaymentViewModels
            {
                Order = order,
                TotalAmount = order.TotalAmount
            };

            // Nếu là Momo (VietQR), tạo URL QR động đúng số tiền
            if (order.PaymentMethod == "Momo")
            {
                vm.VietQrImageUrl = BuildVietQrUrl((long)order.TotalAmount, order.OrderCode);
            }

            return View(vm);
        }

        // ─────────────────────────────────────────────
        //  STAFF: Xác nhận thanh toán (cả Cash và Momo/VietQR)
        // ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee,Manager,Admin")]
        public IActionResult ConfirmPayment(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return NotFound();

            // Cho phép xác nhận cả tiền mặt lẫn chuyển khoản VietQR
            if (order.PaymentMethod != "Cash" && order.PaymentMethod != "Momo")
            {
                TempData["ErrorMessage"] = "Phương thức thanh toán không hợp lệ!";
                return RedirectToAction("Index");
            }

            order.PaymentStatus = "Paid";
            order.Status = "Processing";

            var empIdStr = User.FindFirst("UserId")?.Value;
            if (empIdStr != null)
            {
                var emp = _context.Employees.FirstOrDefault(e => e.UserId == int.Parse(empIdStr));
                order.EmployeeId = emp?.Id;
            }

            _context.SaveChanges();

            var method = order.PaymentMethod == "Momo" ? "chuyển khoản VietQR" : "tiền mặt";
            TempData["SuccessMessage"] = $"Đã xác nhận thanh toán {method} đơn {order.OrderCode}!";
            return RedirectToAction("Index");
        }

        // ─────────────────────────────────────────────
        //  STAFF: Cập nhật trạng thái đơn
        // ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee,Manager,Admin")]
        public IActionResult UpdateStatus(int id, string status)
        {
            var order = _context.Orders.Find(id);
            if (order == null) return NotFound();
            order.Status = status;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // ─────────────────────────────────────────────
        //  STAFF: Xóa đơn
        // ─────────────────────────────────────────────
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager,Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.OrderItemToppings)
                .FirstOrDefault(o => o.Id == id);

            if (order != null)
            {
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
