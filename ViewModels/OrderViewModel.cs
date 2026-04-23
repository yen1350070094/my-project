using MilkTeaShop.Models;

namespace MilkTeaShop.ViewModels
{
    public class CreateOrderViewModel
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public List<ProductSize> Sizes { get; set; } = new();
        public List<Topping> Toppings { get; set; } = new();
        public List<SugarLevel> SugarLevels { get; set; } = new();
        public List<IceLevel> IceLevels { get; set; } = new();

        public int SelectedSizeId { get; set; }
        public List<int> SelectedToppingIds { get; set; } = new();
        public int SelectedSugarLevelId { get; set; }
        public int SelectedIceLevelId { get; set; }
        public int Quantity { get; set; } = 1;
        public string? Note { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerAddress { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
    }

    public class PaymentViewModel
    {
        public Order Order { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}
