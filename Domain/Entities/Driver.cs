using System;
using System.Collections.Generic;
using System.Text;
using static Domain.Entities.Mashwar;

namespace Domain.Entities
{
    internal class Driver
    {
        public long UserId { get; set; }
        public int? CityId { get; set; }
        public bool IsAvailable { get; set; }
        public decimal RatingAvg { get; set; }
        public int RatingCount { get; set; }

        public User User { get; set; } = null!;
        public City? City { get; set; }

        public string? NationalId { get; set; }
        public string? LicenseNumber { get; set; }
        public DateOnly? LicenseExpiry { get; set; } // أو DateTime? إذا ما تستخدم DateOnly
        public string? Iban { get; set; }

        public ICollection<DriverDocument> Documents { get; set; } = new List<DriverDocument>();

        public ICollection<DriverVehicle> Vehicles { get; set; } = new List<DriverVehicle>();
        public ICollection<DriverAvailabilitySlot> AvailabilitySlots { get; set; } = new List<DriverAvailabilitySlot>();
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<Booking> DriverBookings { get; set; } = new List<Booking>();
    }
}
