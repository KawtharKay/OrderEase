using Domain.Enums;

namespace Domain.Entities
{
    public class ReturnRequest : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public string Reason { get; set; } = default!;
        public ReturnRequestStatus Status { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal WalletCreditAmount { get; set; }
        public decimal DebtReductionAmount { get; set; }
        public string? RejectionReason { get; set; }
        public ICollection<ReturnRequestItem> ReturnRequestItems { get; set; } = new HashSet<ReturnRequestItem>();
    }
}
