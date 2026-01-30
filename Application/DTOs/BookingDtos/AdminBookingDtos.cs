using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.BookingDtos
{
    public class AdminBookingDtos
    {
        public record AdminBookingListItemDto(
    long BookingId,
    long TripId,

    DateTime DepartAtUtc,
    decimal Price,

    int FromCityId,
    string FromCityNameAr,
    int ToCityId,
    string ToCityNameAr,

    long DriverUserId,
    string DriverName,

    long CustomerUserId,
    string CustomerName,

    string Status,
    DateTime CreatedAtUtc,
    string? Note
);

        public record AdminBookingDetailsDto(
            long BookingId,
            long TripId,

            DateTime DepartAtUtc,
            decimal Price,

            int FromCityId,
            string FromCityNameAr,
            int ToCityId,
            string ToCityNameAr,

            long DriverUserId,
            string DriverName,
            string DriverPhone,

            long CustomerUserId,
            string CustomerName,
            string CustomerPhone,

            string Status,
            DateTime CreatedAtUtc,
            string? Note
        );

        public record AdminUpdateBookingStatusRequest(
            string Status,
            string? Note
        );

    }
}
