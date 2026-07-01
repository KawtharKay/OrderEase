using Domain.Enums;

namespace Domain.Entities
{
    public class WalletTransaction : BaseEntity
    {
        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; } = default!;
        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }
        public decimal Amount { get; set; }
        public WalletTransactionType Type { get; set; }
        public string? PaystackReference { get; set; }
        public PaystackStatus Status { get; set; }
        public string Description { get; set; } = default!;
    }
}