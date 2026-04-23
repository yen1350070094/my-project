using System.ComponentModel.DataAnnotations;

namespace MilkTeaShop.Models
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // URL ảnh banner (lưu đường dẫn file upload hoặc URL ngoài)
        public string? ImageUrl { get; set; }

        // Link khi click vào banner (ví dụ: /Order/Create?productId=1)
        public string? LinkUrl { get; set; }

        // Thứ tự hiển thị (số nhỏ hiển thị trước)
        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
