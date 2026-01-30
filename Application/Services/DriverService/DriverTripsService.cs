using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.DriverService
{
    using Application.DTOs.Result;
    using Application.Interfaces.IDriver;
    using Domain.Entities;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using static Application.DTOs.Trips.TripsDto;

    public class DriverTripsService : IDriverTripsService
    {
        private readonly MashwarDbContext _db;
        public DriverTripsService(MashwarDbContext db) => _db = db;

        private async Task<Driver?> GetDriverByUserIdAsync(long userId, CancellationToken ct)
            => await _db.Drivers.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.UserId == userId, ct);

        public async Task<ApiResult<List<DriverTripListItemDto>>> GetMyTripsAsync(
            long userId, string? status, int? from, int? to, DateTime? dateFrom, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult<List<DriverTripListItemDto>>.NotFound("Driver not found.");

            var q = _db.Trips.AsNoTracking()
                .Where(t => t.DriverUserId == driver.UserId)
                .Include(t => t.FromCity)
                .Include(t => t.ToCity)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("active", StringComparison.OrdinalIgnoreCase)) q = q.Where(x => x.IsActive);
                else if (status.Equals("inactive", StringComparison.OrdinalIgnoreCase)) q = q.Where(x => !x.IsActive);
            }

            if (from.HasValue) q = q.Where(x => x.FromCityId == from.Value);
            if (to.HasValue) q = q.Where(x => x.ToCityId == to.Value);
            if (dateFrom.HasValue) q = q.Where(x => x.DepartAt >= dateFrom.Value);

            var list = await q
                .OrderByDescending(x => x.DepartAt)
                .Select(x => new DriverTripListItemDto(
                    x.Id,
                    x.FromCityId, x.FromCity.NameAr,
                    x.ToCityId, x.ToCity.NameAr,
                    x.DepartAt,
                    x.Price,
                    x.IsActive
                ))
                .ToListAsync(ct);

            return ApiResult<List<DriverTripListItemDto>>.Ok(list);
        }

        public async Task<ApiResult<long>> CreateTripAsync(long userId, CreateTripDto dto, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult<long>.NotFound("Driver not found.");

            if (dto.FromCityId == dto.ToCityId)
                return ApiResult<long>.Fail("FromCityId and ToCityId cannot be the same.");

            if (dto.DepartAt < DateTime.UtcNow.AddMinutes(1))
                return ApiResult<long>.Fail("DepartAt must be in the future.");

            if (dto.Price <= 0)
                return ApiResult<long>.Fail("Price must be greater than 0.");

            // تحقق من وجود المدن (حتى لو غير نشطة، حسب سياستك)
            var citiesCount = await _db.Cities.IgnoreQueryFilters()
                .Where(c => c.Id == dto.FromCityId || c.Id == dto.ToCityId)
                .CountAsync(ct);

            if (citiesCount != 2)
                return ApiResult<long>.Fail("Invalid city ids.");

            // (اختياري) لو عندك CityRoutes فعّالة، تحقق أنها موجودة
            // var allowed = await _db.CityRoutes.AnyAsync(r => r.IsActive && r.FromCityId == dto.FromCityId && r.ToCityId == dto.ToCityId, ct);
            // if (!allowed) return ApiResult<int>.Fail("Route is not allowed.");

            var trip = new Trip
            {
                DriverUserId = driver.UserId,
                FromCityId = dto.FromCityId,
                ToCityId = dto.ToCityId,
                DepartAt = dto.DepartAt,
                Price = dto.Price,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Trips.Add(trip);
            await _db.SaveChangesAsync(ct);

            return ApiResult<long>.Ok(trip.Id, "Trip created.");
        }

        public async Task<ApiResult> UpdateTripAsync(long userId, int tripId, UpdateTripDto dto, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult.NotFound("Driver not found.");

            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId && t.DriverUserId == driver.UserId, ct);
            if (trip is null) return ApiResult.NotFound("Trip not found.");

            if (dto.FromCityId == dto.ToCityId)
                return ApiResult.Fail("FromCityId and ToCityId cannot be the same.");

            if (dto.DepartAt < DateTime.UtcNow.AddMinutes(1))
                return ApiResult.Fail("DepartAt must be in the future.");

            if (dto.Price <= 0)
                return ApiResult.Fail("Price must be greater than 0.");

            var citiesCount = await _db.Cities.IgnoreQueryFilters()
                .Where(c => c.Id == dto.FromCityId || c.Id == dto.ToCityId)
                .CountAsync(ct);

            if (citiesCount != 2)
                return ApiResult.Fail("Invalid city ids.");

            trip.FromCityId = dto.FromCityId;
            trip.ToCityId = dto.ToCityId;
            trip.DepartAt = dto.DepartAt;
            trip.Price = dto.Price;
            trip.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Trip updated.");
        }

        public async Task<ApiResult> ActivateTripAsync(long userId, int tripId, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult.NotFound("Driver not found.");

            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId && t.DriverUserId == driver.UserId, ct);
            if (trip is null) return ApiResult.NotFound("Trip not found.");

            trip.IsActive = true;
            trip.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Trip activated.");
        }

        public async Task<ApiResult> DeactivateTripAsync(long userId, int tripId, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(userId, ct);
            if (driver is null) return ApiResult.NotFound("Driver not found.");

            var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId && t.DriverUserId == driver.UserId, ct);
            if (trip is null) return ApiResult.NotFound("Trip not found.");

            trip.IsActive = false;
            trip.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Trip deactivated.");
        }
    }

}
