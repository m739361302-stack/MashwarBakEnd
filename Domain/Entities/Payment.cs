using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static Domain.Entities.Mashwar;

namespace Domain.Entities
{
    internal class Payment
    {
        public long Id { get; set; }
        public string PaymentCode { get; set; } = null!;
        public long BookingId { get; set; }
        public long CustomerUserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public string? Provider { get; set; }
        public string? ProviderReference { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Note { get; set; }

        public Booking Booking { get; set; } = null!;
        public User Customer { get; set; } = null!;
        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}

