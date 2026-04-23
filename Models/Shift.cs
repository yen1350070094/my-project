using System.ComponentModel.DataAnnotations;

namespace MilkTeaShop.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;   // Ca sáng | Ca chiều | Ca tối

        // Lưu dạng string "HH:mm" để tương thích tốt với EF Core 8 + SQL Server
        [MaxLength(5)]
        public string StartTime { get; set; } = "06:00";

        [MaxLength(5)]
        public string EndTime { get; set; } = "14:00";

        // Navigation
        public ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
    }
}
