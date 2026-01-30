using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
namespace Application.DTOs.Auth
{


    public class RegisterDriverRequest
    {
        // Account
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public string Password { get; set; } = null!;
        public string City { get; set; } = null!;

        // Driver
        public string NationalId { get; set; } = null!;
        public string LicenseNumber { get; set; } = null!;
        public DateTime? LicenseExpiry { get; set; }
        public string? Iban { get; set; }

        // Vehicle
        public string? CarMake { get; set; }
        public string? CarModel { get; set; }
        public int? CarYear { get; set; }
        public string? PlateNumber { get; set; }
        public string? CarColor { get; set; }

        // Files (Optional)
        public IFormFile? NationalIdFile { get; set; }
        public IFormFile? LicenseFile { get; set; }
        public IFormFile? CarRegFile { get; set; }
    }


    public class RegisterDriverResponse
    {
        public long DriverId { get; set; }
        public int ApprovalStatus { get; set; } // 0 Pending, 1 Approved, 2 Rejected (حسب نظامك)
        public string Message { get; set; } = "Request submitted.";
    }

}
