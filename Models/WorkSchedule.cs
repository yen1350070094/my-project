using System.ComponentModel.DataAnnotations;

namespace MilkTeaShop.Models
{
    public class WorkSchedule
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public int ShiftId { get; set; }

        // Lưu dạng DateTime để tương thích tốt EF Core 8
        public DateTime WorkDate { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        [MaxLength(255)]
        public string? Note { get; set; }

        // Navigation
        public Employee Employee { get; set; } = null!;
        public Shift Shift { get; set; } = null!;
    }
}
