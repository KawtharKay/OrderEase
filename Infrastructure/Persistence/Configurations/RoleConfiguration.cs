using Application.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasData(
                new Role
                {
                    Id = Guid.Parse(""),
                    Name = AppRoles.Supplier
                },
                new Role
                {
                    Id = Guid.Parse(""),
                    Name = AppRoles.Customer
                }
            );
        }
    }
}