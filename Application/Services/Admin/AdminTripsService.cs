using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.Trips.AdminTripDtos;

namespace Application.Services.Admin
{
    public class AdminTripsService
    {
        private readonly MashwarDbContext _db;

        public AdminTripsService(MashwarDbContext db)
        {
            _db = db;
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
