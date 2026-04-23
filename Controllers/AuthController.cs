using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MilkTeaShop.Data;
using MilkTeaShop.Models;
using MilkTeaShop.ViewModels;
using System.Security.Claims;
using System.Text;

namespace MilkTeaShop.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AuthController(ApplicationDbContext context) => _context = context;

        // GET /Auth/Login
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST /Auth/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == vm.Email && u.IsActive);

            if (user == null || !VerifyPassword(vm.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(vm);
            }

            await SignInUser(user);

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);
            return RedirectToAction("Index", "Home");
        }

        // GET /Auth/LoginGoogle
        public IActionResult LoginGoogle(string? returnUrl = null)
        {
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleCallback", new { returnUrl })
            };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        // GET /Auth/GoogleCallback
        public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

            var email    = result.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
            var fullName = result.Principal.FindFirstValue(ClaimTypes.Name)  ?? "";
            var googleId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            // Tìm user theo email
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Tự động đăng ký nếu chưa có
                var customerRole = await _context.Roles.FirstAsync(r => r.Name == "Customer");
                user = new User
                {
                    RoleId       = customerRole.Id,
                    FullName     = fullName,
                    Phone        = "",
                    Email        = email,
                    PasswordHash = HashPassword(googleId), // dùng googleId làm password hash
                    IsActive     = true,
                    CreatedAt    = DateTime.Now
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var customer = new Customer
                {
                    UserId    = user.Id,
                    FullName  = fullName,
                    Phone     = "",
                    Email     = email,
                    CreatedAt = DateTime.Now
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Reload với navigation
                user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Customer)
                    .FirstAsync(u => u.Id == user.Id);
            }

            if (!user.IsActive)
            {
                TempData["ErrorMessage"] = "Tài khoản của bạn đã bị vô hiệu hoá!";
                return RedirectToAction("Login");
            }

            await SignInUser(user);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        // GET /Auth/Register
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            return View(new RegisterViewModel());
        }

        // POST /Auth/Register
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (await _context.Users.AnyAsync(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng");
                return View(vm);
            }

            var customerRole = await _context.Roles.FirstAsync(r => r.Name == "Customer");
            var user = new User
            {
                RoleId       = customerRole.Id,
                FullName     = vm.FullName,
                Phone        = vm.Phone,
                Email        = vm.Email,
                PasswordHash = HashPassword(vm.Password),
                IsActive     = true,
                CreatedAt    = DateTime.Now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var customer = new Customer
            {
                UserId    = user.Id,
                FullName  = vm.FullName,
                Phone     = vm.Phone,
                Email     = vm.Email,
                Address   = vm.Address,
                CreatedAt = DateTime.Now
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // POST /Auth/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();

        //  Helpers 
        private async Task SignInUser(User user)
        {
            var customerId = user.Customer?.Id.ToString() ??
                (await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id))?.Id.ToString() ?? "0";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name,           user.FullName),
                new(ClaimTypes.Email,          user.Email),
                new(ClaimTypes.Role,           user.Role.Name),
                new("UserId",                  user.Id.ToString()),
                new("CustomerId",              customerId),
            };
            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        internal static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password + "MilkTeaSalt2025");
            return Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(bytes));
        }

        private static bool VerifyPassword(string password, string hash)
            => HashPassword(password) == hash;
    }
}
