using Application.DTOs.Admin;
using Application.Services.helper;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.Admin.AdminReportsDtos;

namespace Application.Services.Admin
{
    public sealed class AdminReportsService
    {
        private readonly MashwarDbContext _db;

        public AdminReportsService(MashwarDbContext db)
        {
            _db = db;
        }

        public async Task<AdminReportsResponse> GetAdminReportsAsync(AdminReportsQuery q, CancellationToken ct)
        {
            var (fromUtc, toUtcExclusive) = ResolveRangeUtc(q);

            // ---------- Base query: Bookings + Trips + Cities + Users (driver/customer) ----------
            var baseQ =
                from b in _db.Bookings.AsNoTracking()
                join t in _db.Trips.AsNoTracking() on b.TripId equals t.Id
                join fromCity in _db.Cities.AsNoTracking() on t.FromCityId equals fromCity.Id
                join toCity in _db.Cities.AsNoTracking() on t.ToCityId equals toCity.Id
                join d in _db.Users.AsNoTracking() on b.DriverUserId equals d.Id
                join c in _db.Users.AsNoTracking() on b.CustomerUserId equals c.Id
                where b.CreatedAt >= fromUtc && b.CreatedAt < toUtcExclusive
                select new
                {
                    Booking = b,
                    Trip = t,
                    FromCityName = fromCity.NameAr,
                    ToCityName = toCity.NameAr,
                    DriverName = d.FullName,
                    CustomerName = c.FullName
                };

            // Status filter
            if (!string.Equals(q.Status, "all", StringComparison.OrdinalIgnoreCase))
            {
                baseQ = baseQ.Where(x => x.Booking.BookingStatus.ToString() == q.Status);
            }

            // City filters (الفرونت يرسل اسم المدينة)
            if (!string.Equals(q.FromCity, "all", StringComparison.OrdinalIgnoreCase))
            {
                baseQ = baseQ.Where(x => x.FromCityName == q.FromCity);
            }

            if (!string.Equals(q.ToCity, "all", StringComparison.OrdinalIgnoreCase))
            {
                baseQ = baseQ.Where(x => x.ToCityName == q.ToCity);
            }

            // ---------- KPIs ----------
            var totalBookings = await baseQ.CountAsync(ct);

            var confirmedCount = await baseQ.CountAsync(x => x.Booking.BookingStatus.ToString() == "Confirmed", ct);
            var cancelledCount = await baseQ.CountAsync(x => x.Booking.BookingStatus.ToString() == "CancelledByCustomer", ct);
            var rejectedCount = await baseQ.CountAsync(x => x.Booking.BookingStatus.ToString() == "RejectedByDriver", ct);

            var revenue = await baseQ
                .Where(x => x.Booking.BookingStatus.ToString() == "Confirmed")
                .SumAsync(x => (decimal?)x.Booking.PriceSnapshot, ct) ?? 0m;

            var driversActive = await _db.Users.AsNoTracking()
                .CountAsync(u => u.UserType.ToString() == "Driver" && u.IsActive, ct);

            // TripsActive: ضمن نفس الفترة حسب DepartAt
            var tripsActive = await _db.Trips.AsNoTracking()
                .Where(t => t.IsActive && t.DepartAt >= fromUtc && t.DepartAt < toUtcExclusive)
                .CountAsync(ct);

            var kpis = new AdminReportsKpisDto(
                Bookings: totalBookings,
                Confirmed: confirmedCount,
                Cancelled: cancelledCount,
                DriversActive: driversActive,
                TripsActive: tripsActive,
                Revenue: revenue
            );

            // ---------- Summary Rows ----------
            var avgPrice = await baseQ.AverageAsync(x => (decimal?)x.Booking.PriceSnapshot, ct) ?? 0m;

            var summaryRows = new List<LabelValueDto>
        {
            new("إجمالي الحجوزات", totalBookings),
            new("الحجوزات المؤكدة", confirmedCount),
            new("الحجوزات الملغاة", cancelledCount),
            new("الحجوزات المرفوضة", rejectedCount),
            new("متوسط سعر الحجز", $"{Math.Round(avgPrice, 2)} ر.س"),
        };

            // ---------- Bookings rows (آخر 200) ----------
            var bookingsRows = await baseQ
                .OrderByDescending(x => x.Booking.CreatedAt)
                .Take(200)
                .Select(x => new AdminBookingRowDto(
                    Id: x.Booking.Id,
                    From: x.FromCityName,
                    To: x.ToCityName,
                    Price: x.Booking.PriceSnapshot,
                    Status: x.Booking.BookingStatus.ToString(),
                    Driver: x.DriverName,
                    Customer: x.CustomerName,
                    CreatedAtUtc: x.Booking.CreatedAt
                ))
                .ToListAsync(ct);

            // ---------- Trips rows (آخر 200) ----------
            // ملاحظة: هنا نجيب DriverName عبر Join
            var tripRows =
                await (from t in _db.Trips.AsNoTracking()
                       join fromCity in _db.Cities.AsNoTracking() on t.FromCityId equals fromCity.Id
                       join toCity in _db.Cities.AsNoTracking() on t.ToCityId equals toCity.Id
                       join d in _db.Users.AsNoTracking() on t.DriverUserId equals d.Id
                       where t.DepartAt >= fromUtc && t.DepartAt < toUtcExclusive
                       select new { t, FromName = fromCity.NameAr, ToName = toCity.NameAr, DriverName = d.FullName })
                .WhereIf(!string.Equals(q.FromCity, "all", StringComparison.OrdinalIgnoreCase), x => x.FromName == q.FromCity)
                .WhereIf(!string.Equals(q.ToCity, "all", StringComparison.OrdinalIgnoreCase), x => x.ToName == q.ToCity)
                .OrderByDescending(x => x.t.CreatedAt)
                .Take(200)
                .Select(x => new AdminTripRowDto(
                    Id: x.t.Id,
                    From: x.FromName,
                    To: x.ToName,
                    DepartAtUtc: x.t.DepartAt,
                    Price: x.t.Price,
                    Driver: x.DriverName,
                    IsActive: x.t.IsActive
                ))
                .ToListAsync(ct);

            // ---------- Revenue per day (Confirmed only) ----------
            var rows = await baseQ
               .Where(x => x.Booking.BookingStatus == BookingStatus.Confirmed)
               .GroupBy(x => x.Booking.CreatedAt.Date) // ✅ قابل للترجمة (CAST as date)
               .Select(g => new
               {
                   Day = g.Key,
                   Revenue = g.Sum(z => z.Booking.PriceSnapshot),
                   Bookings = g.Count()
               })
               .OrderBy(x => x.Day)
               .ToListAsync(ct);

            // تحويل DateTime -> DateOnly بعد ما تطلع النتائج من SQL
            var revenueRows = rows
                .Select(x => new AdminRevenueRowDto(
                    Day: DateOnly.FromDateTime(x.Day),
                    Revenue: x.Revenue,
                    Bookings: x.Bookings
                ))
                .ToList();

            // ---------- Drivers performance ----------
            // مثال بسيط: إجمالي حجوزات (confirmed) لكل سائق في الفترة + رحلاته المفعلة

            var tripsAgg = _db.Trips
        .GroupBy(t => t.DriverUserId)
        .Select(g => new
        {
            DriverUserId = g.Key,
            TotalTrips = g.Count(),
            ActiveTrips = g.Count(x => x.IsActive)
        });

            var driverRows = await baseQ
                .GroupBy(x => new { x.Booking.DriverUserId, x.DriverName })
                .Select(g => new { Id = g.Key.DriverUserId, Name = g.Key.DriverName })
                .GroupJoin(
                    tripsAgg,
                    d => d.Id,
                    t => t.DriverUserId,
                    (d, tg) => new { d, t = tg.FirstOrDefault() }
                )
                .Select(x => new
                {
                    x.d.Id,
                    x.d.Name,
                    TotalTrips = x.t != null ? x.t.TotalTrips : 0,
                    ActiveTrips = x.t != null ? x.t.ActiveTrips : 0
                })
                .OrderByDescending(x => x.TotalTrips)   // ✅ هنا على int مباشر
                .Take(50)
                .Select(x => new AdminDriverRowDto(
                    Id: x.Id,
                    Name: x.Name,
                    City: "-",
                    ActiveTrips: x.ActiveTrips,
                    TotalTrips: x.TotalTrips,
                    Rating: 4.5,
                    IsActive: true,
                    IsAvailable: true
                ))
                .ToListAsync(ct);

            return new AdminReportsResponse(
                Kpis: kpis,
                SummaryRows: summaryRows,
                BookingsRows: bookingsRows,
                DriverRows: driverRows,
                TripRows: tripRows,
                RevenueRows: revenueRows
            );
        }
        private static (DateTime fromUtc, DateTime toUtcExclusive) ResolveRangeUtc(AdminReportsQuery q)
        {
            var now = DateTime.UtcNow;

            // exclusive end = tomorrow 00:00 UTC for "today"
            if (q.Range.Equals("today", StringComparison.OrdinalIgnoreCase))
            {
                var from = now.Date;
                return (from, from.AddDays(1));
            }

            if (q.Range.Equals("last7", StringComparison.OrdinalIgnoreCase))
            {
                var to = now;
                var from = now.AddDays(-7);
                return (from, to);
            }

            if (q.Range.Equals("last30", StringComparison.OrdinalIgnoreCase))
            {
                var to = now;
                var from = now.AddDays(-30);
                return (from, to);
            }

            // custom
            if (q.FromDate is null || q.ToDate is null)
            {
                // fallback
                var to = now;
                var from = now.AddDays(-7);
                return (from, to);
            }

            var fromUtc = q.FromDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var toUtc = q.ToDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1); // inclusive day
            return (fromUtc, toUtc);
        }


        public async Task<byte[]> ExportCsvAsync(AdminReportsFilterDto f, CancellationToken ct)
        {
            // 1) جهّز range تواريخ
            var (from, to) = ResolveRange(f);

            // 2) Query مثال لتفاصيل الحجوزات (عدّل حسب جداولك)
            var q = _db.Bookings.AsNoTracking()
                .Where(b => b.CreatedAt >= from && b.CreatedAt < to);

            if (!string.IsNullOrWhiteSpace(f.Status) && f.Status != "all")
            {
                // إذا عندك Enum BookingStatus قارن بالـEnum مباشرة (أفضل)
                q = q.Where(b => b.BookingStatus.ToString() == f.Status);
            }

            // مثال: فلترة من/إلى عبر Trip (عدّل حسب علاقاتك)
            // نفترض Booking لديه TripId و Trip يحتوي FromCityId/ToCityId
            q = q.Where(b => b.TripId != null);

            var rows = await q
                .Select(b => new
                {
                    BookingId = b.Id,
                    TripId = b.TripId,
                    Status = b.BookingStatus.ToString(),
                    Price = b.PriceSnapshot,
                    CreatedAt = b.CreatedAt,
                    DriverUserId = b.DriverUserId,
                    CustomerUserId = b.CustomerUserId
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

            // 3) CSV
            var sb = new StringBuilder();
            sb.AppendLine("BookingId,TripId,Status,Price,CreatedAt,DriverUserId,CustomerUserId");

            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(",",
                    Csv(r.BookingId),
                    Csv(r.TripId),
                    Csv(r.Status),
                    Csv(r.Price),
                    Csv(r.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")),
                    Csv(r.DriverUserId),
                    Csv(r.CustomerUserId)
                ));
            }

            // UTF8 with BOM (مهم لفتح عربي في Excel)
            var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            return utf8Bom.GetBytes(sb.ToString());
        }

        private static string Csv(object? v)
        {
            if (v == null) return "";
            var s = v.ToString() ?? "";

            // Escape quotes & commas & newlines
            if (s.Contains('"')) s = s.Replace("\"", "\"\"");
            if (s.Contains(',') || s.Contains('\n') || s.Contains('\r') || s.Contains('"'))
                s = $"\"{s}\"";

            return s;
        }

        private static (DateTime from, DateTime to) ResolveRange(AdminReportsFilterDto f)
        {
            var now = DateTime.UtcNow;
            DateTime from;
            DateTime to = now;

            switch (f.Range)
            {
                case "today":
                    from = now.Date;
                    to = now.Date.AddDays(1);
                    break;

                case "last30":
                    from = now.Date.AddDays(-30);
                    to = now.Date.AddDays(1);
                    break;

                case "custom":
                    // DateOnly? -> DateTime
                    var fd = f.FromDate ?? DateOnly.FromDateTime(now.Date.AddDays(-7));
                    var td = f.ToDate ?? DateOnly.FromDateTime(now.Date);
                    from = fd.ToDateTime(TimeOnly.MinValue);
                    to = td.ToDateTime(TimeOnly.MinValue).AddDays(1);
                    break;

                case "last7":
                default:
                    from = now.Date.AddDays(-7);
                    to = now.Date.AddDays(1);
                    break;
            }

            return (from, to);
        }
    }
}
