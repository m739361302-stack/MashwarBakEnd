using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.SearchTripsDto
{
    public record SearchTripsQueryDto(
      int FromCityId,
      int ToCityId,
      DateTime? Date // يوم محدد (اختياري)
  );

    public record TripSearchItemDto(
        long TripId,
        long DriverId,
        string DriverName,
        decimal Price,
        DateTime DepartAt,
        int FromCityId,
        string FromCityNameAr,
        int ToCityId,
        string ToCityNameAr,
        bool DriverIsAvailable
    );

}
