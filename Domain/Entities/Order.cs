using Domain.Enums;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = default!;
        public Guid CustomerId { get; set; } 
        public Customer Customer { get; set; } = default!;
        public OrderStatus OrderStatus { get; set; }
        public decimal ItemsSubtotal { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal WalletAmountUsed { get; set; }
        public decimal AmountOwed { get; set; }
        public bool DeliveryFeeConfirmed { get; set; } = true;
        public DateTime OrderDate { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new HashSet<OrderStatusHistory>();
        public ICollection<DeliveryCharge> DeliveryCharges { get; set; } = new HashSet<DeliveryCharge>();
    }
}
