using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MilkTeaShop.Models
{
    public class OrderItemTopping
    {
        [Key]
        public int Id { get; set; }

        public int OrderItemId { get; set; }
        public int ToppingId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }   // Giá topping tại thời điểm đặt

        // Navigation
        public OrderItem OrderItem { get; set; } = null!;
        public Topping Topping { get; set; } = null!;
    }
}
