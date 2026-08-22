namespace Domain.Entities
{
    public class StockMovement : BaseEntity
    {
        public Guid ItemId { get; set; }
        public Item Item { get; set; } = default!;
        public int QuantityAdded { get; set; }
        public decimal CostPrice { get; set; }
        public string? Reference { get; set; }
        public DateTime DateReceived { get; set; }
    }
}
