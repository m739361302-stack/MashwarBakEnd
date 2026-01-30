using Application.DTOs.Result;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.CityRoutes.CityRoutesDtos;

namespace Application.Interfaces.ICity
{
    public interface ICityRouteService
    {
        Task<ApiResult<List<CityRouteListItemDto>>> GetAdminRoutesAsync(bool includeInactive, CancellationToken ct);
        Task<ApiResult<long>> CreateAsync(CityRouteCreateDto dto, CancellationToken ct);
        Task<ApiResult> UpdateAsync(int id, CityRouteUpdateDto dto, CancellationToken ct);
    }

}
