using Application.DTOs.Auth;
using Application.DTOs.DriverApprovalDtos;
using Application.Interfaces.Admin;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Admin
{
    public class AdminDriverApprovalsService : IAdminDriverApprovalsService
    {
        private readonly MashwarDbContext _db;

        public AdminDriverApprovalsService(MashwarDbContext db)
        {
            _db = db;
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
    }
}
