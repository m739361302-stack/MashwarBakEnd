using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Auth
{
    public record LoginRequestDto(string Phone, string Password);

    public record AuthUserDto(
        long Id,
        string FullName,
        string Phone,
        string? Email,
        byte UserType,
        byte ApprovalStatus,
        bool IsActive
    );

    public record LoginResponseDto(
        string AccessToken,
        DateTime ExpiresAtUtc,
        AuthUserDto User
    );

    public record MeResponseDto(AuthUserDto User);

    public record RegisterDriverRequestDto2(
        string FullName,
        string Phone,
        string? Email,
        string Password,
        int? CityId,

        // Driver fields
        string NationalId,
        string LicenseNumber,
        DateOnly? LicenseExpiry,
        string? Iban,

        // Vehicle (optional)
        string? CarMake,
        string? CarModel,
        short? CarYear,
        string? PlateNumber,
        string? CarColor
    );

    public class RegisterDriverRequestDto
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Email { get; set; }
        public string Password { get; set; } = null!;

        public int CityId { get; set; }

        public string NationalId { get; set; } = null!;
        public string LicenseNumber { get; set; } = null!;
        public DateOnly? LicenseExpiry { get; set; }
        public string? Iban { get; set; }

        public string? CarMake { get; set; }
        public string? CarModel { get; set; }
        public short? CarYear { get; set; }
        public string? PlateNumber { get; set; }
        public string? CarColor { get; set; }

        // ✅ Files (Optional)
        public IFormFile? NationalIdFile { get; set; }
        public IFormFile? LicenseFile { get; set; }
        public IFormFile? CarRegFile { get; set; }
    }

    public record ApiOkDto(string Message);

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }

    public class ChangePasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "Password changed.";
    }

}
