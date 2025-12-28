using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using static Domain.Entities.Mashwar;

namespace Domain.Entities
{
    internal class Booking
    {

        public long Id { get; set; }
        public long TripId { get; set; }
        public long CustomerUserId { get; set; }
        public long DriverUserId { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public decimal PriceSnapshot { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }

        public Trip Trip { get; set; } = null!;
        public User Customer { get; set; } = null!;
        public Driver Driver { get; set; } = null!;

        public ICollection<BookingStatusHistory> StatusHistory { get; set; } = new List<BookingStatusHistory>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
