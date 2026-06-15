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
                    Id = Guid.Parse("6E7AF04D-5B6C-4177-A81A-DF253D35441F"),
                    Name = AppRoles.Supplier
                },
                new Role
                {
                    Id = Guid.Parse("7E64770B-A73A-40EF-BCFE-5659C3B61EE5"),
                    Name = AppRoles.Customer
                }
            );
        }
    }
}