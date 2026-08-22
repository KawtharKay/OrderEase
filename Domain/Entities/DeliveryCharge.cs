namespace Domain.Entities
{
    public class DeliveryCharge : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public string Label { get; set; } = default!;
        public decimal Amount { get; set; }
    }
}
