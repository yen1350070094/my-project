using System.ComponentModel.DataAnnotations;

namespace MilkTeaShop.Models
{
    public class IceLevel
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Label { get; set; } = string.Empty;  // Không đá | Ít đá | Nửa đá | Full đá

        public int Percentage { get; set; }                // 0 | 30 | 50 | 100

        // Navigation
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
