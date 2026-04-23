using System.ComponentModel.DataAnnotations;

namespace MilkTeaShop.Models
{
    public class SugarLevel
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Label { get; set; } = string.Empty;  // Không đường | Ít ngọt | Nửa ngọt | Vừa | Bình thường

        public int Percentage { get; set; }                // 0 | 30 | 50 | 70 | 100

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
