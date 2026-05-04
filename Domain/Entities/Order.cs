using Domain.Enums;

namespace Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = default!;
        public Guid CustomerId { get; set; } 
        public Customer Customer { get; set; } = default!;
        public OrderStatus OrderStatus { get; set; } 
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    }
}
