using Domain.Enums;

namespace Domain.Entities
{
    public class Customer : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Address { get; set; } = default!;
        public PaymentStatus PaymentStatus { get; set; }
        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    }
}
