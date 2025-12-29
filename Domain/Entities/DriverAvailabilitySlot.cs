using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class DriverAvailabilitySlot
    {
        public long Id { get; set; }
        public long DriverUserId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public Driver Driver { get; set; } = null!;
    }
}
