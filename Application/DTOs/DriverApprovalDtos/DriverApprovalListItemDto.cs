using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.DriverApprovalDtos
{
    public record DriverApprovalListItemDto(
        long DriverUserId,
        string FullName,
        string Phone,
        string? Email,
        int? CityId,
        string? CityNameAr,
        byte ApprovalStatus,
        DateTime CreatedAtUtc,

        // Driver profile
        string? NationalId,
        string? LicenseNumber,
        DateOnly? LicenseExpiry,
        string? Iban
    );

    public record PagedResultDto<T>(
        int Page,
        int PageSize,
        int Total,
        List<T> Items
    );

    public record ApproveDriverRequestDto(string? Note);

    public record RejectDriverRequestDto(string Reason);

}
