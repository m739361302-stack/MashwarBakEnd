using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Refund
    {
        public long Id { get; set; }
        public long PaymentId { get; set; }
        public decimal Amount { get; set; }
        public RefundStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? Reason { get; set; }

        public Payment Payment { get; set; } = null!;
    }
}
