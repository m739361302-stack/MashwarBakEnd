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
    using static Application.DTOs.Cities.CitiesDtos;

    public class CityService : ICityService
    {
        private readonly MashwarDbContext _db;

        public CityService(MashwarDbContext db) => _db = db;

        public async Task<ApiResult<List<CityListItemDto>>> GetAdminCitiesAsync(bool includeInactive, CancellationToken ct)
        {
            var q = _db.Cities.AsNoTracking();

            if (includeInactive)
                q = q.IgnoreQueryFilters();

            var items = await q
                .OrderBy(x => x.NameAr)
                .Select(x => new CityListItemDto(x.Id, x.NameAr, x.NameEn, x.IsActive))
                .ToListAsync(ct);

            return ApiResult<List<CityListItemDto>>.Ok(items);
        }

        public async Task<ApiResult<List<CityListItemDto>>> GetPublicCitiesAsync(CancellationToken ct)
        {
            // يستفيد من QueryFilter (IsActive=true)
            var items = await _db.Cities.AsNoTracking()
                .OrderBy(x => x.NameAr)
                .Select(x => new CityListItemDto(x.Id, x.NameAr, x.NameEn, x.IsActive))
                .ToListAsync(ct);

            return ApiResult<List<CityListItemDto>>.Ok(items);
        }

        public async Task<ApiResult<CityListItemDto>> CreateAsync(CityCreateDto dto, CancellationToken ct)
        {
            var nameAr = (dto.NameAr ?? "").Trim();
            var nameEn = dto.NameEn?.Trim();

            if (string.IsNullOrWhiteSpace(nameAr))
                return ApiResult<CityListItemDto>.Fail("NameAr is required.");

            // منع التكرار (حتى لو غير نشطة، ممكن تعالجها بطريقتين)
            var exists = await _db.Cities.IgnoreQueryFilters()
                .AnyAsync(x => x.NameAr == nameAr, ct);

            if (exists)
                return ApiResult<CityListItemDto>.Fail("City name already exists.");

            var cityinsert = new  Domain.Entities.City
            {
                NameAr = nameAr,
                NameEn = nameEn,
                IsActive = true,
                //CreatedAt = DateTime.UtcNow
            };

            _db.Cities.Add(cityinsert);
            await _db.SaveChangesAsync(ct);

            return ApiResult<CityListItemDto>.Ok(
                new CityListItemDto(cityinsert.Id, cityinsert.NameAr, cityinsert.NameEn, cityinsert.IsActive),
                "City created."
            );
        }

        public async Task<ApiResult> UpdateAsync(int id, CityUpdateDto dto, CancellationToken ct)
        {
            var nameAr = (dto.NameAr ?? "").Trim();
            var nameEn = dto.NameEn?.Trim();

            if (string.IsNullOrWhiteSpace(nameAr))
                return ApiResult.Fail("NameAr is required.");

            var city = await _db.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (city is null)
                return ApiResult.NotFound("City not found.");

            var nameTaken = await _db.Cities.IgnoreQueryFilters()
                .AnyAsync(x => x.Id != id && x.NameAr == nameAr, ct);

            if (nameTaken)
                return ApiResult.Fail("City name already exists.");

            city.NameAr = nameAr;
            city.NameEn = nameEn;
            city.IsActive = dto.IsActive;
            //city.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("City updated.");
        }

        public async Task<ApiResult> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var city = await _db.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (city is null)
                return ApiResult.NotFound("City not found.");

            // اختياري: منع تعطيل مدينة لها Routes نشطة
            var hasActiveRoutes = await _db.CityRoutes.IgnoreQueryFilters()
                .AnyAsync(r => r.IsActive && (r.FromCityId == id || r.ToCityId == id), ct);

            if (hasActiveRoutes)
                return ApiResult.Fail("Cannot deactivate city with active routes.");

            city.IsActive = false;
            //city.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("City deactivated.");
        }


        public async Task<List<AdminCityListItemDto>> GetAllAsync(CancellationToken ct)
        {
            return await _db.Cities.AsNoTracking()
                .OrderBy(x => x.NameAr) // أو Name
                .Select(x => new AdminCityListItemDto(
                    x.Id,
                    x.NameAr,
                    x.IsActive,
                    x.CreatedAt
                ))
                .ToListAsync(ct);
        }

        public async Task<AdminCityListItemDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await _db.Cities.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new AdminCityListItemDto(x.Id, x.NameAr, x.IsActive, x.CreatedAt))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<AdminCityListItemDto> CreateAsync(AdminCreateCityRequest req, CancellationToken ct)
        {
            var name = (req.NameAr ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("City name is required.");

            var exists = await _db.Cities.AnyAsync(x => x.NameAr == name, ct);
            if (exists)
                throw new InvalidOperationException("City already exists.");

            var city = new City
            {
                NameAr = name,
                IsActive = req.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _db.Cities.Add(city);
            await _db.SaveChangesAsync(ct);

            return new AdminCityListItemDto(city.Id, city.NameAr, city.IsActive, city.CreatedAt);
        }

        public async Task<bool> UpdateAsync(int id, AdminUpdateCityRequest req, CancellationToken ct)
        {
            var city = await _db.Cities.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (city == null) return false;

            var name = (req.NameAr ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("City name is required.");

            var exists = await _db.Cities.AnyAsync(x => x.Id != id && x.NameAr == name, ct);
            if (exists)
                throw new InvalidOperationException("City name already used.");

            city.NameAr = name;
            city.IsActive = req.IsActive;
            city.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id, CancellationToken ct)
        {
            var city = await _db.Cities.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (city == null) return false;

            city.IsActive = !city.IsActive;
            city.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        {
            var city = await _db.Cities.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (city == null) return false;

            _db.Cities.Remove(city);
            await _db.SaveChangesAsync(ct);
            return true;
        }

    }

}
