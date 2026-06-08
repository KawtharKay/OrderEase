using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(70);
            builder.Property(a => a.HashPassword)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasData(new User
            {
                Id = Guid.Parse("BE31038D-70A8-4F1E-845F-111B2EC46E60"),
                Email = "admin@gmail.com",
                HashPassword = "",
                CreatedBy = "admin@gmail.com",
                DateCreated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
