using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MilkTeaShop.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống"), MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá gốc không được để trống"), Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Navigation - Thêm dấu ? để tránh lỗi ModelState.IsValid bị False
        public virtual Category? Category { get; set; }
        public virtual ICollection<ProductSize>? Sizes { get; set; } = new List<ProductSize>();
        public virtual ICollection<OrderItem>? OrderItems { get; set; } = new List<OrderItem>();
    }
}