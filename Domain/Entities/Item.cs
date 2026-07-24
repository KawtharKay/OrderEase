namespace Domain.Entities
{
    public class Item : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string ImageUrl { get; set; } = default!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsAvailable { get; set; }
        public byte[] RowVersion { get; set; } = default!;
        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    }
}
