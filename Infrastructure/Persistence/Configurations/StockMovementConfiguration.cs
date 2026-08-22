using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.QuantityAdded)
                .IsRequired();

            builder.Property(x => x.CostPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Reference)
                .HasMaxLength(200);

            builder.Property(x => x.DateReceived)
                .IsRequired();

            builder.HasOne(x => x.Item)
                .WithMany(x => x.StockMovements)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.ItemId, x.DateReceived });
        }
    }
}
