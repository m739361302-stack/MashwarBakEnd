using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.BookingDtos
{
    public record CreateBookingDto(int TripId, int PassengersCount = 1, string? Note = null);

    public record BookingListItemDto(
        long BookingId,
        BookingStatus Status,
        long TripId,
        DateTime DepartAt,
        decimal Price,
        int FromCityId, string FromCityNameAr,
        int ToCityId, string ToCityNameAr,
        long DriverId,
        string DriverName,
        string customerName,
        int PassengersCount,
        DateTime CreatedAt
    );

}
