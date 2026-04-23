using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkTeaShop.Data;
using MilkTeaShop.Models;

namespace MilkTeaShop.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CustomerController(ApplicationDbContext context) => _context = context;

        // GET /Customer
        public IActionResult Index()
        {
            var customers = _context.Customers
                .Include(c => c.Orders)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();
            return View(customers);
        }

        // GET /Customer/Details/5
        public IActionResult Details(int id)
        {
            var customer = _context.Customers
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefault(c => c.Id == id);

            if (customer == null) return NotFound();
            return View(customer);
        }

        // GET /Customer/Edit/5
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST /Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();
            if (!ModelState.IsValid) return View(customer);

            _context.Customers.Update(customer);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
