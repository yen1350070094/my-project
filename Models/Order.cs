using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MilkTeaShop.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public int? CustomerId { get; set; }
        public int? EmployeeId { get; set; }

        [Required, MaxLength(30)]
        public string OrderCode { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
        // Pending | Processing | Completed | Cancelled

        [MaxLength(20)]
        public string PaymentMethod { get; set; } = "Cash";
        // Cash | Momo

        [MaxLength(20)]
        public string PaymentStatus { get; set; } = "Unpaid";
        // Unpaid | Paid | Refunded

        public string? Note { get; set; }
        public string? CancelReason { get; set; }       // Lý do hủy
        public DateTime OrderedAt { get; set; } = DateTime.Now;
        public DateTime? CancelledAt { get; set; }

        public Customer? Customer { get; set; }
        public Employee? Employee { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
