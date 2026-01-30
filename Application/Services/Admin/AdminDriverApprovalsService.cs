using Application.DTOs.Admin;
using Application.DTOs.Auth;
using Application.DTOs.DriverApprovalDtos;
using Application.Interfaces;
using Application.Interfaces.Admin;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using static Application.DTOs.Admin.AdminDriverDtos;
using static Application.DTOs.DriverDto.AdminDriverDtos;

namespace Application.Services.Admin
{
    public class AdminDriverApprovalsService : IAdminDriverApprovalsService
    {
        private readonly MashwarDbContext _db;
        private readonly IFileStorage _fileStorage;


        public AdminDriverApprovalsService(MashwarDbContext db, IFileStorage fileStorage)
        {
            _db = db;
            _fileStorage = fileStorage;
        }

        public async Task<PagedResultDto<DriverApprovalListItemDto>> GetApprovalsAsync(string status, int? cityId, string? q, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 5 or > 100 ? 20 : pageSize;

            var query = _db.Users.AsNoTracking()
                .Where(u => u.UserType == UserType.Driver);

            // status
            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim().ToLowerInvariant();
                if (status == "pending") query = query.Where(u => u.ApprovalStatus == ApprovalStatus.Pending);
                else if (status == "approved") query = query.Where(u => u.ApprovalStatus == ApprovalStatus.Approved);
                else if (status == "rejected") query = query.Where(u => u.ApprovalStatus == ApprovalStatus.Rejected);
                // "all" => no filter
            }

            // join Drivers + Cities
            var joined = query
                .Join(_db.Drivers.AsNoTracking(), u => u.Id, d => d.UserId, (u, d) => new { u, d })
                .GroupJoin(_db.Cities.AsNoTracking(), x => x.d.CityId, c => c.Id, (x, cg) => new { x.u, x.d, city = cg.FirstOrDefault() });

            if (cityId.HasValue)
                joined = joined.Where(x => x.d.CityId == cityId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                joined = joined.Where(x =>
                    x.u.FullName.Contains(q) ||
                    x.u.Phone.Contains(q) ||
                    (x.u.Email != null && x.u.Email.Contains(q)) ||
                    (x.d.LicenseNumber != null && x.d.LicenseNumber.Contains(q)) ||
                    (x.d.NationalId != null && x.d.NationalId.Contains(q))
                );
            }

            var total = await joined.CountAsync();

            var items = await joined
                .OrderByDescending(x => x.u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DriverApprovalListItemDto(
                    x.u.Id,
                    x.u.FullName,
                    x.u.Phone,
                    x.u.Email,
                    x.d.CityId,
                    x.city != null ? x.city.NameAr : null,
                    (byte)x.u.ApprovalStatus,
                    x.u.CreatedAt,
                    x.d.NationalId,
                    x.d.LicenseNumber,
                    x.d.LicenseExpiry,
                    x.d.Iban
                ))
                .ToListAsync();

            return new PagedResultDto<DriverApprovalListItemDto>(page, pageSize, total, items);
        }

        public async Task<ApiOkDto> ApproveAsync(long driverUserId, long adminUserId, string? note)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == driverUserId && x.UserType == UserType.Driver);
            if (user == null) throw new InvalidOperationException("السائق غير موجود");

            user.ApprovalStatus = ApprovalStatus.Approved;
            user.ApprovalReviewedAt = DateTime.UtcNow;
            user.ApprovalReviewedByUserId = adminUserId;
            user.ApprovalNote = string.IsNullOrWhiteSpace(note) ? "تمت الموافقة" : note.Trim();

            await _db.SaveChangesAsync();
            return new ApiOkDto("تم اعتماد السائق بنجاح");
        }

        public async Task<ApiOkDto> RejectAsync(long driverUserId, long adminUserId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("سبب الرفض مطلوب");

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == driverUserId && x.UserType == UserType.Driver);
            if (user == null) throw new InvalidOperationException("السائق غير موجود");

            user.ApprovalStatus = ApprovalStatus.Rejected;
            user.ApprovalReviewedAt = DateTime.UtcNow;
            user.ApprovalReviewedByUserId = adminUserId;
            user.ApprovalNote = reason.Trim();

            await _db.SaveChangesAsync();
            return new ApiOkDto("تم رفض طلب السائق");
        }


        public async Task<AdminDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct)
        {
            var pendingBookings = await _db.Bookings
                .CountAsync(b => b.BookingStatus == BookingStatus.PendingDriverConfirm, ct);

            var activeTrips = await _db.Trips
                .CountAsync(t => t.IsActive, ct);

            var inactiveDrivers = await _db.Drivers
                .CountAsync(d => d.User.ApprovalStatus != ApprovalStatus.Approved, ct);

            return new AdminDashboardSummaryDto(
                pendingBookings,
                activeTrips,
                inactiveDrivers
            );
        }


        public async Task<List<AdminDriverListItemDto>> GetAdminDriversAsync(CancellationToken ct)
        {
            var q = _db.Drivers
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.City)
                .Include(d => d.Vehicles); // لو عندك DriverVehicles navigation

            var list = await q
                .OrderByDescending(d => d.User.CreatedAt)
                .Select(d => new AdminDriverListItemDto(
                    d.UserId,
                    d.User.FullName,
                    d.User.Phone,
                    d.CityId,
                    d.City != null ? d.City.NameAr : null,
                    d.User.IsActive,
                    d.IsAvailable,
                    d.RatingAvg,
                    d.RatingCount,

                    // Trips stats (حسب جداولك)
                    TotalTrips: _db.Trips.Count(t => t.DriverUserId == d.UserId),
                    CompletedTrips: _db.Trips.Count(t => t.DriverUserId == d.UserId /*&& t.Status == TripStatus.Completed*/),
                    CancelledTrips: _db.Trips.Count(t => t.DriverUserId == d.UserId /*&& t.Status == TripStatus.Cancelled*/),

                    d.User.LastLoginAt, // أو LastSeenAt إذا عندك
                    d.User.CreatedAt,

                    d.Vehicles
                        .OrderByDescending(v => v.IsPrimary)
                        .Select(v => new AdminDriverCarDto(v.Make, v.Model, v.Year, v.PlateNumber, v.Color))
                        .FirstOrDefault()
                ))
                .ToListAsync(ct);

            return list;
        }


        private static string Mask(string? v, int keepStart = 2, int keepEnd = 2)
        {
            if (string.IsNullOrWhiteSpace(v)) return "—";
            v = v.Trim();
            if (v.Length <= keepStart + keepEnd) return new string('*', v.Length);
            return v.Substring(0, keepStart) + new string('*', v.Length - keepStart - keepEnd) + v.Substring(v.Length - keepEnd);
        }

        private static string MapStatus(ApprovalStatus s)
            => s == ApprovalStatus.Pending ? "Pending"
             : s == ApprovalStatus.Approved ? "Approved"
             : "Rejected";

        public async Task<List<AdminDriverApprovalListItemDto>> GetDriverApprovalsAsync(CancellationToken ct)
        {
            var q = _db.Drivers.AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.City);

            var list = await q
                .OrderByDescending(d => d.User.CreatedAt)
                .Select(d => new AdminDriverApprovalListItemDto(
                    d.UserId,
                    MapStatus(d.User.ApprovalStatus),
                    d.User.CreatedAt,
                    d.User.FullName,
                    d.User.Phone,
                    d.User.Email,
                    d.CityId,
                    d.City != null ? d.City.NameAr : null,
                    Mask(d.NationalId, 2, 2),
                    d.LicenseNumber,
                    d.LicenseExpiry,
                    Mask(d.Iban, 2, 2)
                ))
                .ToListAsync(ct);

            return list;
        }

        public async Task<AdminDriverApprovalDetailsDto?> GetDriverApprovalDetailsAsync(long driverUserId, CancellationToken ct)
        {
            var driver = await _db.Drivers.AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.City)
                .FirstOrDefaultAsync(d => d.UserId == driverUserId, ct);

            if (driver == null) return null;

            var vehicle = await _db.DriverVehicles.AsNoTracking()
                .Where(v => v.DriverUserId == driverUserId && v.IsPrimary)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new AdminVehicleDto(v.Make, v.Model, v.Year, v.PlateNumber, v.Color))
                .FirstOrDefaultAsync(ct);

            var docs = await _db.DriverDocuments.AsNoTracking()
                .Where(x => x.DriverUserId == driverUserId)
                .OrderByDescending(x => x.Created)
                .Select(x => new { x.DocType, x.StoredPath })
                .ToListAsync(ct);

            //var docDtos = docs.Select(d => new AdminDriverDocDto(
            //    d.DocType.ToString(), // لو enum
            //    //""
            //    _fileStorage.GetPublicUrl(d.StoredPath) // رابط عرض/تحميل
            //)).ToList();
            var docDtos = docs
                    .ToList()
                    .Select(d => new AdminDriverDocDto(
                        d.DocType.ToString(),
                        _fileStorage.GetPublicUrl(d.StoredPath)
                    ))
                    .ToList();
            return new AdminDriverApprovalDetailsDto(
                driver.UserId,
                MapStatus(driver.User.ApprovalStatus),
                driver.User.CreatedAt,
                driver.User.FullName,
                driver.User.Phone,
                driver.User.Email,
                driver.CityId,
                driver.City != null ? driver.City.NameAr : null,
               // Mask(driver.NationalId, 2, 2),
                driver.NationalId,
                driver.LicenseNumber,
                driver.LicenseExpiry,
                Mask(driver.Iban, 2, 2),
                vehicle,
                docDtos,
                ""

                //driver.User.AdminNote // لو عندك حقل ملاحظة
            );
        }

        public async Task<bool> ApproveDriverAsync(long driverUserId, string? adminNote, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == driverUserId && x.UserType == UserType.Driver, ct);
            if (user == null) return false;

            user.ApprovalStatus = ApprovalStatus.Approved;
            user.IsActive = true;
            //user.AdminNote = string.IsNullOrWhiteSpace(adminNote) ? "تمت الموافقة" : adminNote.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> RejectDriverAsync(long driverUserId, string reason, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == driverUserId && x.UserType == UserType.Driver, ct);
            if (user == null) return false;

            user.ApprovalStatus = ApprovalStatus.Rejected;
            user.IsActive = false;
            //user.AdminNote = reason.Trim();
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

    }
}
