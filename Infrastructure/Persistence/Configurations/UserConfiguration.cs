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
            builder.Property(x => x.Salt)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(u => u.VerificationToken)
                .IsRequired(false)
                .HasMaxLength(200);
            builder.Property(u => u.VerificationTokenExpiry)
                .IsRequired(false);
            builder.Property(u => u.IsVerified)
                .IsRequired()
                .HasDefaultValue(false);
            builder.Property(a => a.PasswordResetToken)
                .IsRequired(false)
                .HasMaxLength(6);
            builder.Property(a => a.ResetTokenExpiry)
                .IsRequired(false);
        }
    }
}
