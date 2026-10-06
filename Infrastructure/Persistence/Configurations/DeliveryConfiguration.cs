using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
    {
        public void Configure(EntityTypeBuilder<Delivery> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DeliveryMethod)
                .IsRequired();

            builder.Property(x => x.DeliveryAddress)
                .HasMaxLength(500);

            builder.HasOne(x => x.DeliveryLocation)
                .WithMany()
                .HasForeignKey(x => x.DeliveryLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Order)
                .WithOne()
                .HasForeignKey<Delivery>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}