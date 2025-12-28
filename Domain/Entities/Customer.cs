using System;
using System.Collections.Generic;
using System.Text;
using static Domain.Entities.Mashwar;

namespace Domain.Entities
{
    internal class Customer
    {
        public long UserId { get; set; }
        public int? CityId { get; set; }
        public string? AvatarUrl { get; set; }

        public User User { get; set; } = null!;
        public City? City { get; set; }
    }
}
