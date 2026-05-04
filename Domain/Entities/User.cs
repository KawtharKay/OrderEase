namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = default!;
        public string HashPassword { get; set; } = default!;
        public ICollection<UserRole> UserRoles { get; set; } = new HashSet<UserRole>();
    }
}
