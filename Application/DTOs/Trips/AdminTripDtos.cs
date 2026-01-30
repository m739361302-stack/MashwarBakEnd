using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Trips
{
    public class AdminTripDtos
    {
        public record AdminTripListItemDto(
    long TripId,
    bool IsActive,
    DateTime DepartAtUtc,
    decimal Price,
    int Seats,

    int FromCityId,
    string FromCityNameAr,
    int ToCityId,
    string ToCityNameAr,

    long DriverUserId,
    string DriverName,

    DateTime CreatedAtUtc,
    string? Note
);

        public record AdminTripDetailsDto(
            long TripId,
            bool IsActive,
            DateTime DepartAtUtc,
            decimal Price,
            int Seats,

            int FromCityId,
            string FromCityNameAr,
            int ToCityId,
            string ToCityNameAr,

            long DriverUserId,
            string DriverName,
            string? DriverPhone,

            DateTime CreatedAtUtc,
            string? Note
        );

        public record AdminSetTripActiveRequest(bool IsActive);
    }
}
