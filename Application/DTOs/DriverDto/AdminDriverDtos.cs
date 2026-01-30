using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.DriverDto
{
    public class AdminDriverDtos
    {
        public record AdminDriverApprovalListItemDto(
            long DriverUserId,
            string Status,              // Pending / Approved / Rejected
            DateTime CreatedAtUtc,
            string FullName,
            string Phone,
            string? Email,
            int? CityId,
            string? CityNameAr,
            string NationalIdMasked,
            string LicenseNumber,
            DateOnly? LicenseExpiry,
            string? IbanMasked
        );

        public record AdminDriverApprovalDetailsDto(
            long DriverUserId,
            string Status,
            DateTime CreatedAtUtc,
            string FullName,
            string Phone,
            string? Email,
            int? CityId,
            string? CityNameAr,
            string NationalIdMasked,
            string LicenseNumber,
            DateOnly? LicenseExpiry,
            string? IbanMasked,
            AdminVehicleDto? Vehicle,
            List<AdminDriverDocDto> Docs,
            string? AdminNote
        );

        public record AdminVehicleDto(string? Make, string? Model, int? Year, string? PlateNumber, string? Color);

        public record AdminDriverDocDto(string DocType, string Url);

        public record AdminApproveDriverRequest(string? AdminNote);
        public record AdminRejectDriverRequest(string Reason);

    }
}
