using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.CityS
{
    using Application.DTOs.Result;
    using Application.Interfaces.ICity;
    using Domain.Entities;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using static Application.DTOs.CityRoutes.CityRoutesDtos;

    public class CityRouteService : ICityRouteService
    {
        private readonly MashwarDbContext _db;
        public CityRouteService(MashwarDbContext db) => _db = db;

        public async Task<ApiResult<List<CityRouteListItemDto>>> GetAdminRoutesAsync(bool includeInactive, CancellationToken ct)
        {
            var q = _db.CityRoutes.AsNoTracking();

            if (includeInactive)
                q = q.IgnoreQueryFilters();

            var items = await q
                .Include(x => x.FromCity)
                .Include(x => x.ToCity)
                .OrderBy(x => x.FromCity.NameAr).ThenBy(x => x.ToCity.NameAr)
                .Select(x => new CityRouteListItemDto(
                    x.Id,
                    x.FromCityId, x.FromCity.NameAr,
                    x.ToCityId, x.ToCity.NameAr,
                    x.IsActive
                ))
                .ToListAsync(ct);

            return ApiResult<List<CityRouteListItemDto>>.Ok(items);
        }

        public async Task<ApiResult<long>> CreateAsync(CityRouteCreateDto dto, CancellationToken ct)
        {
            if (dto.FromCityId == dto.ToCityId)
                return ApiResult<long>.Fail("FromCityId and ToCityId cannot be the same.");

            // تحقق من وجود المدن
            var count = await _db.Cities.IgnoreQueryFilters()
                .Where(c => c.Id == dto.FromCityId || c.Id == dto.ToCityId)
                .CountAsync(ct);

            if (count != 2)
                return ApiResult<long>.Fail("Invalid city ids.");

            // منع تكرار المسار
            var exists = await _db.CityRoutes.IgnoreQueryFilters()
                .AnyAsync(r => r.FromCityId == dto.FromCityId && r.ToCityId == dto.ToCityId, ct);

            if (exists)
                return ApiResult<long>.Fail("Route already exists.");

            var route = new CityRoute
            {
                FromCityId = dto.FromCityId,
                ToCityId = dto.ToCityId,
                IsActive = true,
                //CreatedAt = DateTime.UtcNow
            };

            _db.CityRoutes.Add(route);
            await _db.SaveChangesAsync(ct);

            return ApiResult<long>.Ok(route.Id, "Route created.");
        }

        public async Task<ApiResult> UpdateAsync(int id, CityRouteUpdateDto dto, CancellationToken ct)
        {
            if (dto.FromCityId == dto.ToCityId)
                return ApiResult.Fail("FromCityId and ToCityId cannot be the same.");

            var route = await _db.CityRoutes.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (route is null)
                return ApiResult.NotFound("Route not found.");

            // تحقق من وجود المدن
            var count = await _db.Cities.IgnoreQueryFilters()
                .Where(c => c.Id == dto.FromCityId || c.Id == dto.ToCityId)
                .CountAsync(ct);
            if (count != 2)
                return ApiResult.Fail("Invalid city ids.");

            var duplicate = await _db.CityRoutes.IgnoreQueryFilters()
                .AnyAsync(r => r.Id != id && r.FromCityId == dto.FromCityId && r.ToCityId == dto.ToCityId, ct);

            if (duplicate)
                return ApiResult.Fail("Route already exists.");

            route.FromCityId = dto.FromCityId;
            route.ToCityId = dto.ToCityId;
            route.IsActive = dto.IsActive;
            //route.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Route updated.");
        }
    }

}
