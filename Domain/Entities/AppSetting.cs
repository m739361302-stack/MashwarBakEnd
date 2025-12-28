using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class AppSetting
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public string? ValueType { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long? UpdatedByUserId { get; set; }

        public User? UpdatedByUser { get; set; }
    }
}
