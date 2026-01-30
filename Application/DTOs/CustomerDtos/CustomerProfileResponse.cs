using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CustomerDtos
{
    public class CustomerProfileResponse
    {
        public long Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }
}
