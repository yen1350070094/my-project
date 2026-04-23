using System.ComponentModel.DataAnnotations;

namespace MilkTeaShop.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public int? UserId { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(150), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public int LoyaltyPoints { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public User? User { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
