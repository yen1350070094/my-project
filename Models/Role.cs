using System.ComponentModel.DataAnnotations;

namespace MilkTeaShop.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;   // Admin | Employee | Customer

        public string? Description { get; set; }

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
