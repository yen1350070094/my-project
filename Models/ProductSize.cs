using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MilkTeaShop.Models
{
    public class ProductSize
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Required, MaxLength(10)]
        public string SizeName { get; set; } = string.Empty;  // S | M | L

        [Column(TypeName = "decimal(10,2)")]
        public decimal ExtraPrice { get; set; } = 0;

        // Navigation
        public Product Product { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
