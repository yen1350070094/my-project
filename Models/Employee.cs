using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MilkTeaShop.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(50)]
        public string Position { get; set; } = string.Empty;  // Cashier | Barista | Manager

        [Column(TypeName = "decimal(10,2)")]
        public decimal HourlySalary { get; set; }

        public DateTime HireDate { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
