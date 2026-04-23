using MilkTeaShop.Models;

namespace MilkTeaShop.ViewModels
{
    public class PaymentViewModels
    {
        public Order Order { get; set; } = null!;
        public decimal TotalAmount { get; set; }

        // Thêm dòng này vào
        public string? VietQrImageUrl { get; set; }
    }
}