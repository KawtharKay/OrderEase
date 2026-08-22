using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PreviousStatus);

            builder.Property(x => x.NewStatus)
                .IsRequired();

            builder.Property(x => x.ChangedAt)
                .IsRequired();

            builder.HasOne(x => x.Order)
                .WithMany(x => x.StatusHistory)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.OrderId, x.ChangedAt });
        }
    }
}
