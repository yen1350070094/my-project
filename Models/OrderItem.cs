using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MilkTeaShop.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int ProductSizeId { get; set; }
        public int SugarLevelId { get; set; }
        public int IceLevelId { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }   // Giá tại thời điểm đặt

        [Column(TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; }

        public string? Note { get; set; }

        // Navigation
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public ProductSize ProductSize { get; set; } = null!;
        public SugarLevel SugarLevel { get; set; } = null!;
        public IceLevel IceLevel { get; set; } = null!;
        public ICollection<OrderItemTopping> OrderItemToppings { get; set; } = new List<OrderItemTopping>();
    }
}
