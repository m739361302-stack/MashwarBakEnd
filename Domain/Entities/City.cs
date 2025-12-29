using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class City
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = null!;
        public string? NameEn { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }

        public ICollection<CityRoute> FromRoutes { get; set; } = new List<CityRoute>();
        public ICollection<CityRoute> ToRoutes { get; set; } = new List<CityRoute>();
    }
}
