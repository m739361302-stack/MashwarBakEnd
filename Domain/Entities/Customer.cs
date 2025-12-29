using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Customer
    {
        public long UserId { get; set; }
        public int? CityId { get; set; }
        public string? AvatarUrl { get; set; }

        public User User { get; set; } = null!;
        public City? City { get; set; }
    }
}
