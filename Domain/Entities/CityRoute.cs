using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    internal class CityRoute
    {
        public long Id { get; set; }
        public int FromCityId { get; set; }
        public int ToCityId { get; set; }
        public bool IsActive { get; set; }

        public City FromCity { get; set; } = null!;
        public City ToCity { get; set; } = null!;
    }
}
