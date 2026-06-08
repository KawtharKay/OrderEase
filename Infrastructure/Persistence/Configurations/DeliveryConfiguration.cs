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

            builder.HasOne(x => x.Order)
                .WithOne()
                .HasForeignKey<Delivery>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}