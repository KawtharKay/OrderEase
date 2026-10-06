using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DeliveryLocationConfiguration : IEntityTypeConfiguration<DeliveryLocation>
    {
        public void Configure(EntityTypeBuilder<DeliveryLocation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Fee)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
