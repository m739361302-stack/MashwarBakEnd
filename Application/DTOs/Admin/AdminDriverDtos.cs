using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Admin
{
    public class AdminDriverDtos
    {
        public record AdminDriverListItemDto(
                long UserId,
                string FullName,
                string Phone,
                int? CityId,
                string? CityNameAr,
                bool IsActive,
                bool IsAvailableNow,
                decimal RatingAvg,
                int RatingCount,
                int TotalTrips,
                int CompletedTrips,
                int CancelledTrips,
                DateTime? LastSeenAtUtc,
                DateTime CreatedAtUtc,
                AdminDriverCarDto? Car
            );

        public record AdminDriverCarDto(
            string? Make,
            string? Model,
            int? Year,
            string? PlateNumber,
            string? Color
        );

        public record AdminToggleActiveRequest(bool IsActive);
        public record AdminToggleAvailabilityRequest(bool IsAvailableNow); // اختياري
    }
}
