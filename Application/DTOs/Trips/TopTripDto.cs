using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Trips
{
    public record TopTripDto(
       long TripId,
       DateTime DepartAt,
       decimal Price,
       int Seats,
       long DriverUserId,
       string DriverName,
       decimal DriverRating,
       int FromCityId,
       string FromCityNameAr,
       int ToCityId,
       string ToCityNameAr
   );
}
