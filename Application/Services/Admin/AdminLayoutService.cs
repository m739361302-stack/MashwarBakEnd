using Application.DTOs.Admin;
using Domain.Entities;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.Admin
{
    public sealed class AdminLayoutService
    {
        private readonly MashwarDbContext _db;
        //private readonly IUserContext _userContext; // انت عندك غالباً GetUserIdFromContext()

        public AdminLayoutService(MashwarDbContext db/*, IUserContext userContext*/)
        {
            _db = db;
            //_userContext = userContext;
        }

        public async Task<AdminLayoutDto> GetAdminLayoutAsync(CancellationToken ct,long adminId)
        {
           // var userId = User.GetUserId();
            // ===== Admin User =====
           // var adminId = User.UserId; // أو GetUserIdFromContext()
            var adminUser = await _db.Users.AsNoTracking()
                .Where(u => u.Id == adminId)
                .Select(u => new AdminLayoutUserDto(
                    Id: u.Id,
                    Name: u.FullName,
                    Role: u.UserType.ToString()
                ))
                .FirstOrDefaultAsync(ct);

            // fallback لو ما وجد (مثلاً توكن غير موجود)
            adminUser ??= new AdminLayoutUserDto(0, "Admin", "Admin");

            // ===== Counters =====
            var pendingBookings = await _db.Bookings.AsNoTracking()
                .CountAsync(b => b.BookingStatus.ToString() == "PendingDriverConfirm", ct);

            var activeTrips = await _db.Trips.AsNoTracking()
                .CountAsync(t => t.IsActive, ct);

            // inactive drivers: حسب تصميمك.
            // أسهل شيء: Users.UserType == Driver && !IsActive
            var inactiveDrivers = await _db.Users.AsNoTracking()
                .CountAsync(u => u.UserType.ToString() == "Driver" && !u.IsActive, ct);

            var counters = new AdminLayoutCountersDto(
                PendingBookings: pendingBookings,
                ActiveTrips: activeTrips,
                InactiveDrivers: inactiveDrivers
            );

            return new AdminLayoutDto(adminUser, counters);
        }
    }
}
