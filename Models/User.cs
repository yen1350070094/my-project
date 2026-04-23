using System.ComponentModel.DataAnnotations;

namespace MilkTeaShop.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public int RoleId { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Role Role { get; set; } = null!;
        public Employee? Employee { get; set; }
        public Customer? Customer { get; set; }
    }
}
