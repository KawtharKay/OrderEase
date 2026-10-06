namespace Domain.Entities
{
    public class DeliveryLocation : BaseEntity
    {
        public string Name { get; set; } = default!;
        public decimal Fee { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
