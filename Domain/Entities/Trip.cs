using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Trip
    {
        public long Id { get; set; }
        public long DriverUserId { get; set; }
        public int FromCityId { get; set; }
        public int ToCityId { get; set; }
        public DateTime DepartAt { get; set; }
        public decimal Price { get; set; }
        public byte Seats { get; set; } = 1;
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Driver Driver { get; set; } = null!;
        public City FromCity { get; set; } = null!;
        public City ToCity { get; set; } = null!;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
