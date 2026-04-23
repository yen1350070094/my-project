using Microsoft.EntityFrameworkCore;
using MilkTeaShop.Models;
using System.Security.Cryptography;
using System.Text;

namespace MilkTeaShop.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Banner> Banners { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Topping> Toppings { get; set; }
        public DbSet<SugarLevel> SugarLevels { get; set; }
        public DbSet<IceLevel> IceLevels { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemTopping> OrderItemToppings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User).WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User).WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer).WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Employee).WithMany(e => e.Orders)
                .HasForeignKey(o => o.EmployeeId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order).WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductSize).WithMany(ps => ps.OrderItems)
                .HasForeignKey(oi => oi.ProductSizeId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.SugarLevel).WithMany(s => s.OrderItems)
                .HasForeignKey(oi => oi.SugarLevelId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.IceLevel).WithMany(i => i.OrderItems)
                .HasForeignKey(oi => oi.IceLevelId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItemTopping>()
                .HasOne(oit => oit.OrderItem).WithMany(oi => oi.OrderItemToppings)
                .HasForeignKey(oit => oit.OrderItemId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItemTopping>()
                .HasOne(oit => oit.Topping).WithMany(t => t.OrderItemToppings)
                .HasForeignKey(oit => oit.ToppingId).OnDelete(DeleteBehavior.Restrict);

            // ══ SEED DATA ══
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin",    Description = "Quản trị viên" },
                new Role { Id = 2, Name = "Manager",  Description = "Quản lý cửa hàng" },
                new Role { Id = 3, Name = "Employee", Description = "Nhân viên" },
                new Role { Id = 4, Name = "Customer", Description = "Khách hàng" }
            );

            // Tài khoản mẫu (password: 123456)
            var adminHash    = HashPwd("123456");
            var managerHash  = HashPwd("123456");
            var employeeHash = HashPwd("123456");

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, RoleId = 1, FullName = "Admin",        Email = "admin@milktea.com",    Phone = "0900000001", PasswordHash = adminHash,    IsActive = true, CreatedAt = new DateTime(2025,1,1) },
                new User { Id = 2, RoleId = 2, FullName = "Quản lý",      Email = "manager@milktea.com",  Phone = "0900000002", PasswordHash = managerHash,  IsActive = true, CreatedAt = new DateTime(2025,1,1) },
                new User { Id = 3, RoleId = 3, FullName = "Nhân viên A",  Email = "nhanvien@milktea.com", Phone = "0900000003", PasswordHash = employeeHash, IsActive = true, CreatedAt = new DateTime(2025,1,1) }
            );

            modelBuilder.Entity<Employee>().HasData(
                new Employee { Id = 1, UserId = 1, Position = "Admin",    HourlySalary = 0,     HireDate = new DateTime(2025,1,1) },
                new Employee { Id = 2, UserId = 2, Position = "Manager",  HourlySalary = 50000, HireDate = new DateTime(2025,1,1) },
                new Employee { Id = 3, UserId = 3, Position = "Cashier",  HourlySalary = 35000, HireDate = new DateTime(2025,1,1) }
            );

            modelBuilder.Entity<Shift>().HasData(
                new Shift { Id = 1, Name = "Ca sáng",  StartTime = "06:00", EndTime = "14:00" },
                new Shift { Id = 2, Name = "Ca chiều", StartTime = "14:00", EndTime = "22:00" },
                new Shift { Id = 3, Name = "Ca tối",   StartTime = "17:00", EndTime = "23:00" }
            );

            modelBuilder.Entity<SugarLevel>().HasData(
                new SugarLevel { Id = 1, Label = "Không đường", Percentage = 0   },
                new SugarLevel { Id = 2, Label = "Ít ngọt",     Percentage = 30  },
                new SugarLevel { Id = 3, Label = "Nửa ngọt",    Percentage = 50  },
                new SugarLevel { Id = 4, Label = "Vừa ngọt",    Percentage = 70  },
                new SugarLevel { Id = 5, Label = "Bình thường",  Percentage = 100 }
            );

            modelBuilder.Entity<IceLevel>().HasData(
                new IceLevel { Id = 1, Label = "Không đá", Percentage = 0   },
                new IceLevel { Id = 2, Label = "Ít đá",    Percentage = 30  },
                new IceLevel { Id = 3, Label = "Nửa đá",   Percentage = 50  },
                new IceLevel { Id = 4, Label = "Full đá",  Percentage = 100 }
            );

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Trà sữa",      Description = "Các loại trà sữa", IsActive = true },
                new Category { Id = 2, Name = "Trà trái cây", Description = "Trà kết hợp trái cây", IsActive = true },
                new Category { Id = 3, Name = "Smoothie",     Description = "Sinh tố xay", IsActive = true },
                new Category { Id = 4, Name = "Cà phê sữa",   Description = "Cà phê pha sữa", IsActive = true }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, CategoryId = 1, Name = "Trà sữa truyền thống", BasePrice = 35000, IsAvailable = true },
                new Product { Id = 2, CategoryId = 1, Name = "Trà sữa matcha",       BasePrice = 45000, IsAvailable = true },
                new Product { Id = 3, CategoryId = 1, Name = "Trà sữa oolong",       BasePrice = 40000, IsAvailable = true },
                new Product { Id = 4, CategoryId = 2, Name = "Trà vải lychee",        BasePrice = 38000, IsAvailable = true },
                new Product { Id = 5, CategoryId = 2, Name = "Trà đào cam sả",        BasePrice = 40000, IsAvailable = true },
                new Product { Id = 6, CategoryId = 3, Name = "Smoothie xoài",         BasePrice = 45000, IsAvailable = false },
                new Product { Id = 7, CategoryId = 4, Name = "Bạc sỉu",               BasePrice = 30000, IsAvailable = true }
            );

            modelBuilder.Entity<ProductSize>().HasData(
                new ProductSize { Id = 1,  ProductId = 1, SizeName = "S", ExtraPrice = 0     },
                new ProductSize { Id = 2,  ProductId = 1, SizeName = "M", ExtraPrice = 5000  },
                new ProductSize { Id = 3,  ProductId = 1, SizeName = "L", ExtraPrice = 10000 },
                new ProductSize { Id = 4,  ProductId = 2, SizeName = "S", ExtraPrice = 0     },
                new ProductSize { Id = 5,  ProductId = 2, SizeName = "M", ExtraPrice = 5000  },
                new ProductSize { Id = 6,  ProductId = 2, SizeName = "L", ExtraPrice = 10000 },
                new ProductSize { Id = 7,  ProductId = 3, SizeName = "S", ExtraPrice = 0     },
                new ProductSize { Id = 8,  ProductId = 3, SizeName = "M", ExtraPrice = 5000  },
                new ProductSize { Id = 9,  ProductId = 3, SizeName = "L", ExtraPrice = 10000 },
                new ProductSize { Id = 10, ProductId = 4, SizeName = "S", ExtraPrice = 0     },
                new ProductSize { Id = 11, ProductId = 4, SizeName = "M", ExtraPrice = 5000  },
                new ProductSize { Id = 12, ProductId = 4, SizeName = "L", ExtraPrice = 10000 },
                new ProductSize { Id = 13, ProductId = 5, SizeName = "S", ExtraPrice = 0     },
                new ProductSize { Id = 14, ProductId = 5, SizeName = "M", ExtraPrice = 5000  },
                new ProductSize { Id = 15, ProductId = 5, SizeName = "L", ExtraPrice = 10000 },
                new ProductSize { Id = 16, ProductId = 6, SizeName = "S", ExtraPrice = 0     },
                new ProductSize { Id = 17, ProductId = 6, SizeName = "M", ExtraPrice = 5000  },
                new ProductSize { Id = 18, ProductId = 6, SizeName = "L", ExtraPrice = 10000 },
                new ProductSize { Id = 19, ProductId = 7, SizeName = "S", ExtraPrice = 0     },
                new ProductSize { Id = 20, ProductId = 7, SizeName = "M", ExtraPrice = 5000  },
                new ProductSize { Id = 21, ProductId = 7, SizeName = "L", ExtraPrice = 10000 }
            );

            modelBuilder.Entity<Topping>().HasData(
                new Topping { Id = 1, Name = "Trân châu đen",   Price = 5000,  IsAvailable = true },
                new Topping { Id = 2, Name = "Trân châu trắng", Price = 5000,  IsAvailable = true },
                new Topping { Id = 3, Name = "Thạch dừa",       Price = 5000,  IsAvailable = true },
                new Topping { Id = 4, Name = "Pudding trứng",   Price = 8000,  IsAvailable = true },
                new Topping { Id = 5, Name = "Kem phô mai",      Price = 10000, IsAvailable = true },
                new Topping { Id = 6, Name = "Thạch cà phê",    Price = 6000,  IsAvailable = true }
            );
        }

        private static string HashPwd(string pwd)
        {
            var bytes = Encoding.UTF8.GetBytes(pwd + "MilkTeaSalt2025");
            return Convert.ToBase64String(SHA256.HashData(bytes));
        }
    }
}
