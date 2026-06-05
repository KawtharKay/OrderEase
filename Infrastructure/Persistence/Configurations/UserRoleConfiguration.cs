using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(a => new { a.UserId, a.RoleId });

            builder.HasOne(a => a.User)
                .WithMany(a => a.UserRoles)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(a => a.Role)
                .WithMany(a => a.UserRoles)
                .HasForeignKey(a => a.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(new UserRole
            {
                RoleId = Guid.Parse(""),
                UserId = Guid.Parse("")
            });
        }
    }
}
