using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class BookingStatusHistory
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public BookingStatus OldStatus { get; set; }
        public BookingStatus NewStatus { get; set; }
        public long? ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Notes { get; set; }

        public Booking Booking { get; set; } = null!;
        public User? ChangedByUser { get; set; }
    }
}
