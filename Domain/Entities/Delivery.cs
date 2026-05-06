using Domain.Enums;

namespace Domain.Entities
{
    public class Delivery : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public DeliveryMethod DeliveryMethod { get; set; }
    }
}
