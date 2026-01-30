using Application.DTOs.Result;
using Application.DTOs.SearchTripsDto;
using Application.DTOs.Trips;
using Application.Interfaces.TripsSearch;
using Domain.Entities;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.Trips.AdminTripDtos;

namespace Application.Services.TripsSearch
{
    public class TripsSearchService : ITripsSearchService
    {
        private readonly MashwarDbContext _db;
        public TripsSearchService(MashwarDbContext db) => _db = db;

        public async Task<ApiResult<List<TripSearchItemDto>>> SearchAsync(
            long? userId,
            SearchTripsQueryDto q,
            bool requireAvailabilitySlot,
            CancellationToken ct)
        {
            if (q.FromCityId <= 0 || q.ToCityId <= 0)
                return ApiResult<List<TripSearchItemDto>>.Fail("FromCityId and ToCityId are required.");

            if (q.FromCityId == q.ToCityId)
                return ApiResult<List<TripSearchItemDto>>.Fail("FromCityId and ToCityId cannot be the same.");

            var now = DateTime.UtcNow;

            // تاريخ اليوم (إذا العميل حدد يوم فقط)
            DateTime? dayStart = null;
            DateTime? dayEnd = null;
            if (q.Date.HasValue)
            {
                var d = q.Date.Value.Date;
                dayStart = d;
                dayEnd = d.AddDays(1);
            }

            // IMPORTANT: إن كانت عندك ApprovalStatus في Driver
            // افترض: Approved = 1
            const int Approved = 1;

            var tripsQ = _db.Trips.AsNoTracking()
                .Include(t => t.FromCity)
                .Include(t => t.ToCity)
                .Include(t => t.Driver)
                .Where(t =>
                    t.IsActive &&
                    t.FromCityId == q.FromCityId &&
                    t.ToCityId == q.ToCityId &&
                    t.DepartAt >= now &&
                    t.Driver.IsAvailable
                );

            // لو عندك اعتماد السائق
            // احذف الشرط إذا ما عندك ApprovalStatus
            //tripsQ = tripsQ.Where(t => t.Driver.ApprovalStatus == Approved);

            // فلترة حسب اليوم إذا تم تحديده
            if (dayStart.HasValue)
                tripsQ = tripsQ.Where(t => t.DepartAt >= dayStart.Value && t.DepartAt < dayEnd!.Value);

            // شرط Slots (اختياري)
            if (requireAvailabilitySlot)
            {
                tripsQ = tripsQ.Where(t =>
                    _db.DriverAvailabilitySlots.Any(s =>
                        s.DriverUserId == t.DriverUserId &&
                        s.IsActive &&
                        t.DepartAt >= s.StartAt &&
                        t.DepartAt <= s.EndAt
                    )
                );
            }

            // DriverName: لو عندك اسم السائق داخل Users ربطه
            // مثال: Driver.User.FullName (حسب تصميمك)
            // هنا سأفترض Driver عنده User navigation:
            // t.Driver.User.FullName
            var items = await tripsQ
                .OrderBy(t => t.DepartAt)
                .Select(t => new TripSearchItemDto(
                    t.Id,
                    t.DriverUserId,
                    t.Driver.User.FullName,   // <-- عدّلها حسب مشروعك إن لم توجد
                    t.Price,
                    t.DepartAt,
                    t.FromCityId,
                    t.FromCity.NameAr,
                    t.ToCityId,
                    t.ToCity.NameAr,
                    t.Driver.IsAvailable
                ))
                .ToListAsync(ct);

            return ApiResult<List<TripSearchItemDto>>.Ok(items);
        }


        public async Task<List<TopTripDto>> GetTopTripsAsync(
         int? fromCityId,
         int? toCityId,
         DateTime? date,
         int limit,
         CancellationToken ct)
        {
            IQueryable<Trip> q = _db.Trips.AsNoTracking();

            // Filters
            q = q.Where(t => t.IsActive && t.DepartAt > DateTime.UtcNow);

            if (fromCityId.HasValue)
                q = q.Where(t => t.FromCityId == fromCityId.Value);

            if (toCityId.HasValue)
                q = q.Where(t => t.ToCityId == toCityId.Value);

            if (date.HasValue)
            {
                var d = date.Value.Date;
                q = q.Where(t => t.DepartAt.Date == d);
            }

            // Include AFTER filters (أفضل أداء)
            q = q
                .Include(t => t.Driver).ThenInclude(d => d.User)
                .Include(t => t.FromCity)
                .Include(t => t.ToCity);

            // limit
            if (limit <= 0) limit = 3;

            return await q
                .OrderBy(t => t.DepartAt)
                .Take(limit)
                .Select(t => new TopTripDto(
                    t.Id,
                    t.DepartAt,
                    t.Price,
                    t.Seats,
                    t.DriverUserId,
                    t.Driver.User.FullName,
                    t.Driver.RatingAvg,
                    t.FromCityId,
                    t.FromCity.NameAr,
                    t.ToCityId,
                    t.ToCity.NameAr
                ))
                .ToListAsync(ct);
        }


        public async Task<List<AdminTripListItemDto>> GetTripsAsync(CancellationToken ct)
        {
            var q = _db.Trips.AsNoTracking()
                .Include(t => t.FromCity)
                .Include(t => t.ToCity)
                .Include(t => t.Driver).ThenInclude(d => d.User);

            var list = await q
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new AdminTripListItemDto(
                    t.Id,
                    t.IsActive,
                    t.DepartAt,
                    t.Price,
                    t.Seats,

                    t.FromCityId,
                    t.FromCity.NameAr,
                    t.ToCityId,
                    t.ToCity.NameAr,

                    t.DriverUserId,
                    t.Driver.User.FullName,

                    t.CreatedAt,
                    t.Notes
                ))
                .ToListAsync(ct);

            return list;
        }

        public async Task<AdminTripDetailsDto?> GetTripDetailsAsync(long tripId, CancellationToken ct)
        {
            var t = await _db.Trips.AsNoTracking()
                .Include(x => x.FromCity)
                .Include(x => x.ToCity)
                .Include(x => x.Driver).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(x => x.Id == tripId, ct);

            if (t == null) return null;

            return new AdminTripDetailsDto(
                t.Id,
                t.IsActive,
                t.DepartAt,
                t.Price,
                t.Seats,

                t.FromCityId,
                t.FromCity.NameAr,
                t.ToCityId,
                t.ToCity.NameAr,

                t.DriverUserId,
                t.Driver.User.FullName,
                t.Driver.User.Phone,

                t.CreatedAt,
                t.Notes
            );
        }

        public async Task<bool> SetTripActiveAsync(long tripId, bool isActive, CancellationToken ct)
        {
            var trip = await _db.Trips.FirstOrDefaultAsync(x => x.Id == tripId, ct);
            if (trip == null) return false;

            // ممنوع تفعيل رحلة منتهية
            if (isActive && trip.DepartAt < DateTime.UtcNow)
                throw new InvalidOperationException("Cannot activate a past trip.");

            trip.IsActive = isActive;
            trip.UpdatedAt = DateTime.UtcNow; // لو عندك UpdatedAt
            await _db.SaveChangesAsync(ct);

            return true;
        }


    }
}
