namespace Domain.Entities
{
    public class Supplier : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Address { get; set; } = default!;
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public ICollection<ReturnRequest> ReturnRequests { get; set; } = new HashSet<ReturnRequest>();
    }
}
