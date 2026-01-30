using Domain.Enums;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using static Application.DTOs.Admin.AdminUserDtos;

namespace APIs.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]
    //[ApiController]
    public class AdminUsersController : ControllerBase
    {
        private readonly MashwarDbContext _db;

        public AdminUsersController(MashwarDbContext db)
        {
            _db = db;
        }

        // GET: /api/admin/users
        [HttpGet("admin/allusers")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var users = await _db.Users
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Phone,
                    x.Email,
                    x.UserType,
                    x.ApprovalStatus,
                    x.IsActive,
                    x.CreatedAt
                })
                .ToListAsync(ct);

            return Ok(users);
        }

        // GET: /api/admin/users/{id}
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken ct)
        {
            var user = await _db.Users
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Phone,
                    x.Email,
                    x.UserType,
                    x.ApprovalStatus,
                    x.IsActive,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync(ct);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        // PUT: /api/admin/users/{id}/activate

        [HttpPut("{id:long}/activate")]
        public async Task<IActionResult> Activate(long id, CancellationToken ct)
        {
            var user = await _db.Users.FindAsync(new object[] { id }, ct);
            if (user == null) return NotFound();

            user.IsActive = true;
            await _db.SaveChangesAsync(ct);

            return Ok(new { message = "User activated" });
        }

        // PUT: /api/admin/users/{id}/deactivate
        [HttpPut("{id:long}/deactivate")]
        public async Task<IActionResult> Deactivate(long id, CancellationToken ct)
        {
            var user = await _db.Users.FindAsync(new object[] { id }, ct);
            if (user == null) return NotFound();

            user.IsActive = false;
            await _db.SaveChangesAsync(ct);

            return Ok(new { message = "User deactivated" });
        }

 
        [HttpGet("admin/users")]
        public async Task<List<AdminUserListItemDto>> GetAdminUsersAsync(CancellationToken ct)
        {
            var q = _db.Users
                .AsNoTracking()
                .Where(u => u.UserType == UserType.Customer)
                .Include(u => u.Customer)
                 .ThenInclude(c => c.City);

            ;
            //.Include(u => u.City);

            var list = await q
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserListItemDto(
                    u.Id,
                    u.FullName,
                    u.Phone,
                    u.Customer.CityId,
                    u.Customer.City != null ? u.Customer.City.NameAr : null,
                    u.IsActive,

                    // bookings stats
                    TotalBookings: _db.Bookings.Count(b => b.CustomerUserId == u.Id),
                    CompletedBookings: _db.Bookings.Count(b =>
                        b.CustomerUserId == u.Id && b.BookingStatus == BookingStatus.Completed),
                    CancelledBookings: _db.Bookings.Count(b =>
                        b.CustomerUserId == u.Id && b.BookingStatus == BookingStatus.RejectedByDriver),

                    u.CreatedAt,
                    u.LastLoginAt
                ))
                .ToListAsync(ct);

            return list;
        }
    }

}
