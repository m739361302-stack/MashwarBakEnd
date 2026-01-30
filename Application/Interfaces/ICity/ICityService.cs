using Application.DTOs.Result;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.Cities.CitiesDtos;

namespace Application.Interfaces.ICity
{
    public interface ICityService
    {
        Task<ApiResult<List<CityListItemDto>>> GetAdminCitiesAsync(bool includeInactive, CancellationToken ct);
        Task<ApiResult<List<CityListItemDto>>> GetPublicCitiesAsync(CancellationToken ct);

        Task<ApiResult<CityListItemDto>> CreateAsync(CityCreateDto dto, CancellationToken ct);
        Task<ApiResult> UpdateAsync(int id, CityUpdateDto dto, CancellationToken ct);

        /// Soft delete => IsActive=false
        Task<ApiResult> SoftDeleteAsync(int id, CancellationToken ct);
        Task<bool> DeleteAsync(int id, CancellationToken ct);
        Task<bool> ToggleActiveAsync(int id, CancellationToken ct);
        Task<bool> UpdateAsync(int id, AdminUpdateCityRequest req, CancellationToken ct);
        Task<AdminCityListItemDto> CreateAsync(AdminCreateCityRequest req, CancellationToken ct);
        Task<AdminCityListItemDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<List<AdminCityListItemDto>> GetAllAsync(CancellationToken ct);

    }

}
