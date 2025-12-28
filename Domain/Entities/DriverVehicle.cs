using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    internal class DriverVehicle
    {
        public long Id { get; set; }
        public long DriverUserId { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public short? Year { get; set; }
        public string? PlateNumber { get; set; }
        public string? Color { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }

        public Driver Driver { get; set; } = null!;
    }
}
