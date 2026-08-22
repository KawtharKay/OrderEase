using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DeliveryChargeConfiguration : IEntityTypeConfiguration<DeliveryCharge>
    {
        public void Configure(EntityTypeBuilder<DeliveryCharge> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Label)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.Order)
                .WithMany(x => x.DeliveryCharges)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
