using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(70);

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(14);

            builder.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(80);

            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<Supplier>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}