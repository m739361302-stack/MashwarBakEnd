using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.Admin.dashboardDtos;

namespace Application.Services.Admin
{
    public sealed class AdminDashboardService
    {
        private readonly MashwarDbContext _db;

        public AdminDashboardService(MashwarDbContext db)
        {
            _db = db;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct)
        {
            var todayUtc = DateTime.Now.Date;
            var tomorrowUtc = todayUtc.AddDays(1);

            // ===== KPI =====
            var totalDrivers = await _db.Users.AsNoTracking()
                .CountAsync(u => u.UserType.ToString() == "Driver", ct);

            var activeDrivers = await _db.Users.AsNoTracking()
                .CountAsync(u => u.UserType.ToString() == "Driver" && u.IsActive, ct);

            var activeTrips = await _db.Trips.AsNoTracking()
                .CountAsync(t => t.IsActive, ct);

            var pendingBookings = await _db.Bookings.AsNoTracking()
                .CountAsync(b => b.BookingStatus.ToString() == "PendingDriverConfirm", ct);

            var revenueToday = await _db.Bookings.AsNoTracking()
                .Where(b => b.BookingStatus== BookingStatus.Confirmed && b.CreatedAt >= todayUtc && b.CreatedAt < tomorrowUtc)
                .SumAsync(b => (decimal?)b.PriceSnapshot, ct) ?? 0m;

            // ===== Latest Bookings (آخر 10) =====
            var latestBookings =
                await (from b in _db.Bookings.AsNoTracking()
                       join t in _db.Trips.AsNoTracking() on b.TripId equals t.Id
                       join fromCity in _db.Cities.AsNoTracking() on t.FromCityId equals fromCity.Id
                       join toCity in _db.Cities.AsNoTracking() on t.ToCityId equals toCity.Id
                       orderby b.CreatedAt descending
                       select new AdminDashboardBookingDto(
                           Id: b.Id,
                           From: fromCity.NameAr,
                           To: toCity.NameAr,
                           Price: b.PriceSnapshot,
                           Status: b.BookingStatus.ToString(),
                           CreatedAtUtc: b.CreatedAt
                       ))
                .Take(10)
                .ToListAsync(ct);

            // ===== Latest Drivers (آخر 10) =====
            // City: إذا عندك Driver.CityId اربطه هنا. حالياً جعلته "-" لو غير موجود.
            // IsAvailable: اربطه بجدول Availability لو موجود.
            var latestDrivers = await _db.Users.AsNoTracking()
                .Where(u => u.UserType.ToString() == "Driver")
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminDashboardDriverDto(
                    Id: u.Id,
                    Name: u.FullName,
                    City: "-",           // TODO: اربطه بمدينته إن وجدت
                    IsActive: u.IsActive,
                    IsAvailable: true    // TODO: اربطه بجدول التوفر إن وجد
                ))
                .Take(10)
                .ToListAsync(ct);

            var kpi = new AdminDashboardKpisDto(
                TotalDrivers: totalDrivers,
                ActiveDrivers: activeDrivers,
                ActiveTrips: activeTrips,
                PendingBookings: pendingBookings,
                RevenueToday: revenueToday
            );

            return new AdminDashboardDto(kpi, latestBookings, latestDrivers);
        }
    }
}
