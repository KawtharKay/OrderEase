namespace Domain.Entities
{
    public class ReturnRequestItem : BaseEntity
    {
        public Guid ReturnRequestId { get; set; }
        public ReturnRequest ReturnRequest { get; set; } = default!;
        public Guid ItemId { get; set; }
        public Item Item { get; set; } = default!;
        public int Quantity { get; set; }
    }
}
