using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Trips
{
    public class TripsDto
    {
        public record DriverTripListItemDto(
            long Id,
            int FromCityId, string FromCityNameAr,
            int ToCityId, string ToCityNameAr,
            DateTime DepartAt,
            decimal Price,
            bool IsActive
        );

        public record CreateTripDto(int FromCityId, int ToCityId, DateTime DepartAt, decimal Price);
        public record UpdateTripDto(int FromCityId, int ToCityId, DateTime DepartAt, decimal Price);

    }
}
