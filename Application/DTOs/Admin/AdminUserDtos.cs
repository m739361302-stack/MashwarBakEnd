using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Admin
{
    public class AdminUserDtos
    {
        public record AdminUserListItemDto(
            long UserId,
            string FullName,
            string Phone,
            int? CityId,
            string? CityNameAr,
            bool IsActive,
            int TotalBookings,
            int CompletedBookings,
            int CancelledBookings,
            DateTime CreatedAtUtc,
            DateTime? LastLoginAtUtc
        );

        public record AdminToggleUserActiveRequest(bool IsActive);
    }
}
