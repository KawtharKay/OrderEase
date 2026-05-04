namespace Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = default!;
        public ICollection<Item> Items { get; set; } = new HashSet<Item>();
    }
}
