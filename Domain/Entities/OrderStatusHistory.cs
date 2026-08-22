using Domain.Enums;

namespace Domain.Entities
{
    public class OrderStatusHistory : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public OrderStatus? PreviousStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}