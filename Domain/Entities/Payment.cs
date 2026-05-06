using Domain.Enums;

namespace Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = default!;
        public decimal AmountPaid { get; set; }
        public decimal AmountTotal { get; set; }
        public decimal OutstandingBalance { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaystackReference { get; set; } = default!;
        public PaystackStatus Status { get; set; }
        //public string ProofOfPaymentUrl { get; set; } = default!;
        public bool IsConfirmed { get; set; }
        public DateTime? DateConfirmed { get; set; }
    }
}
