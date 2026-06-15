using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AmountPaid)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.AmountTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.OutstandingBalance)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.PaymentDate)
                .IsRequired();

            builder.Property(x => x.PaystackReference)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(PaystackStatus.Pending);

            builder.Property(x => x.IsConfirmed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DateConfirmed)
                .IsRequired(false);

            builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}