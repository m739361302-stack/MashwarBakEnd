using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Cities
{
    public class CitiesDtos
    {
        public record CityListItemDto(int Id, string NameAr, string? NameEn, bool IsActive);

        public record CityCreateDto(string NameAr, string? NameEn);

        public record CityUpdateDto(string NameAr, string? NameEn, bool IsActive);

        public record AdminCityListItemDto(
            int Id,
            string NameAr,
            bool IsActive,
            DateTime CreatedAtUtc
        );

        public record AdminCreateCityRequest(
            string NameAr,
            bool IsActive
        );

        public record AdminUpdateCityRequest(
            string NameAr,
            bool IsActive
        );
    }
}
