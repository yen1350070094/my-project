using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkTeaShop.Data;
using MilkTeaShop.Models;
using MilkTeaShop.ViewModels;

namespace MilkTeaShop.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public EmployeeController(ApplicationDbContext context) => _context = context;

        // GET /Employee
        public IActionResult Index()
        {
            var employees = _context.Employees
                .Include(e => e.User).ThenInclude(u => u.Role)
                .Include(e => e.WorkSchedules)
                .OrderBy(e => e.User.FullName)
                .ToList();
            return View(employees);
        }

        // GET /Employee/Details/5
        public IActionResult Details(int id)
        {
            var employee = _context.Employees
                .Include(e => e.User).ThenInclude(u => u.Role)
                .Include(e => e.WorkSchedules).ThenInclude(ws => ws.Shift)
                .Include(e => e.Orders)
                .FirstOrDefault(e => e.Id == id);

            if (employee == null) return NotFound();
            return View(employee);
        }

        // GET /Employee/Create
        public IActionResult Create()
        {
            ViewBag.Roles = _context.Roles
                .Where(r => r.Name == "Employee" || r.Name == "Manager")
                .ToList();
            return View(new CreateEmployeeViewModel());
        }

        // POST /Employee/Create
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(CreateEmployeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _context.Roles
                    .Where(r => r.Name == "Employee" || r.Name == "Manager")
                    .ToList();
                return View(vm);
            }

            // Ki?m tra email trùng
            if (_context.Users.Any(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "Email này ?ã ???c s? d?ng");
                ViewBag.Roles = _context.Roles
                    .Where(r => r.Name == "Employee" || r.Name == "Manager")
                    .ToList();
                return View(vm);
            }

            // T?o User
            var user = new User
            {
                RoleId = vm.RoleId,
                FullName = vm.FullName,
                Phone = vm.Phone,
                Email = vm.Email,
                PasswordHash = HashPassword(vm.Password),
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            _context.Users.Add(user);
            _context.SaveChanges();

            // T?o Employee
            var employee = new Employee
            {
                UserId = user.Id,
                Position = vm.Position,
                HourlySalary = vm.HourlySalary,
                HireDate = vm.HireDate
            };
            _context.Employees.Add(employee);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Thêm nhân viên {vm.FullName} thành công!";
            return RedirectToAction("Index");
        }

        // GET /Employee/Edit/5
        public IActionResult Edit(int id)
        {
            var emp = _context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.Id == id);
            if (emp == null) return NotFound();

            ViewBag.Roles = _context.Roles
                .Where(r => r.Name == "Employee" || r.Name == "Manager")
                .ToList();

            var vm = new CreateEmployeeViewModel
            {
                FullName = emp.User.FullName,
                Phone = emp.User.Phone,
                Email = emp.User.Email,
                RoleId = emp.User.RoleId,
                Position = emp.Position,
                HourlySalary = emp.HourlySalary,
                HireDate = emp.HireDate,
                Password = "",           // ?? tr?ng = không ??i m?t kh?u
                ConfirmPassword = ""
            };
            ViewBag.EmployeeId = id;
            return View(vm);
        }

        // POST /Employee/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CreateEmployeeViewModel vm)
        {
            // B? validate Password khi edit (?? tr?ng = không ??i)
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _context.Roles
                    .Where(r => r.Name == "Employee" || r.Name == "Manager")
                    .ToList();
                ViewBag.EmployeeId = id;
                return View(vm);
            }

            var emp = _context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.Id == id);
            if (emp == null) return NotFound();

            // C?p nh?t User
            emp.User.FullName = vm.FullName;
            emp.User.Phone = vm.Phone;
            emp.User.RoleId = vm.RoleId;
            if (!string.IsNullOrEmpty(vm.Password))
                emp.User.PasswordHash = HashPassword(vm.Password);

            // C?p nh?t Employee
            emp.Position = vm.Position;
            emp.HourlySalary = vm.HourlySalary;
            emp.HireDate = vm.HireDate;

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"C?p nh?t nhân viên {vm.FullName} thành công!";
            return RedirectToAction("Index");
        }

        // POST /Employee/ToggleActive/5
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ToggleActive(int id)
        {
            var emp = _context.Employees.Include(e => e.User).FirstOrDefault(e => e.Id == id);
            if (emp == null) return NotFound();
            emp.User.IsActive = !emp.User.IsActive;
            _context.SaveChanges();
            TempData["SuccessMessage"] = $"{(emp.User.IsActive ? "Kích ho?t" : "Vô hi?u hoá")} tài kho?n {emp.User.FullName}!";
            return RedirectToAction("Index");
        }

        // POST /Employee/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var emp = _context.Employees
                .Include(e => e.User)
                .Include(e => e.WorkSchedules)
                .FirstOrDefault(e => e.Id == id);
            if (emp != null)
            {
                _context.WorkSchedules.RemoveRange(emp.WorkSchedules);
                _context.Employees.Remove(emp);
                _context.Users.Remove(emp.User);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "?ã xóa nhân viên!";
            }
            return RedirectToAction("Index");
        }

        // GET /Employee/Schedule/5
        public IActionResult Schedule(int id)
        {
            var employee = _context.Employees
                .Include(e => e.User)
                .Include(e => e.WorkSchedules).ThenInclude(ws => ws.Shift)
                .FirstOrDefault(e => e.Id == id);

            if (employee == null) return NotFound();
            ViewBag.Shifts = _context.Shifts.ToList();
            return View(employee);
        }

        // POST /Employee/AddSchedule
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AddSchedule(int EmployeeId, int ShiftId, DateTime WorkDate)
        {
            _context.WorkSchedules.Add(new WorkSchedule
            {
                EmployeeId = EmployeeId,
                ShiftId = ShiftId,
                WorkDate = WorkDate
            });
            _context.SaveChanges();
            return RedirectToAction("Schedule", new { id = EmployeeId });
        }

        // POST /Employee/CheckIn/5
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CheckIn(int id)
        {
            var ws = _context.WorkSchedules.Find(id);
            if (ws == null) return NotFound();
            ws.CheckIn = DateTime.Now;
            _context.SaveChanges();
            return RedirectToAction("Schedule", new { id = ws.EmployeeId });
        }

        // POST /Employee/CheckOut/5
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CheckOut(int id)
        {
            var ws = _context.WorkSchedules.Find(id);
            if (ws == null) return NotFound();
            ws.CheckOut = DateTime.Now;
            _context.SaveChanges();
            return RedirectToAction("Schedule", new { id = ws.EmployeeId });
        }

        private static string HashPassword(string pwd)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(pwd + "MilkTeaSalt2025");
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(bytes));
        }
    }
}
