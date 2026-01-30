using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CityRoutes
{
    public class CityRoutesDtos
    {
        public record CityRouteListItemDto(
            long Id,
            int FromCityId, string FromCityNameAr,
            int ToCityId, string ToCityNameAr,
            bool IsActive
        );

        public record CityRouteCreateDto(int FromCityId, int ToCityId);

        public record CityRouteUpdateDto(int FromCityId, int ToCityId, bool IsActive);

    }
}
