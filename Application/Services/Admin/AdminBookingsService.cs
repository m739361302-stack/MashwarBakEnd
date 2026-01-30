using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Application.DTOs.BookingDtos.AdminBookingDtos;

namespace Application.Services.Admin
{
    public class AdminBookingsService
    {
        private readonly MashwarDbContext _db;

        public AdminBookingsService(MashwarDbContext db)
        {
            _db = db;
        }

        public async Task<List<AdminBookingListItemDto>> GetAllAsync(CancellationToken ct)
        {
            return await _db.Bookings.AsNoTracking()
                .Include(b => b.Trip).ThenInclude(t => t.FromCity)
                .Include(b => b.Trip).ThenInclude(t => t.ToCity)
                .Include(b => b.Driver).ThenInclude(d => d.User)
                .Include(b => b.Customer).ThenInclude(c => c.User)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new AdminBookingListItemDto(
                    b.Id,
                    b.TripId,

                    b.Trip.DepartAt,
                    b.Trip.Price,

                    b.Trip.FromCityId,
                    b.Trip.FromCity.NameAr,
                    b.Trip.ToCityId,
                    b.Trip.ToCity.NameAr,

                    b.DriverUserId,
                    b.Driver.User.FullName,

                    b.CustomerUserId,
                    b.Customer.User.FullName,

                    b.BookingStatus.ToString(),
                    b.CreatedAt,
                    b.Note
                ))
                .ToListAsync(ct);
        }

        public async Task<AdminBookingDetailsDto?> GetByIdAsync(long bookingId, CancellationToken ct)
        {
            var b = await _db.Bookings.AsNoTracking()
                .Include(x => x.Trip).ThenInclude(t => t.FromCity)
                .Include(x => x.Trip).ThenInclude(t => t.ToCity)
                .Include(x => x.Driver).ThenInclude(d => d.User)
                .Include(x => x.Customer).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(x => x.Id == bookingId, ct);

            if (b == null) return null;

            return new AdminBookingDetailsDto(
                b.Id,
                b.TripId,

                b.Trip.DepartAt,
                b.Trip.Price,

                b.Trip.FromCityId,
                b.Trip.FromCity.NameAr,
                b.Trip.ToCityId,
                b.Trip.ToCity.NameAr,

                b.DriverUserId,
                b.Driver.User.FullName,
                b.Driver.User.Phone,

                b.CustomerUserId,
                b.Customer.User.FullName,
                b.Customer.User.Phone,

                b.BookingStatus.ToString(),
                b.CreatedAt,
                b.Note
            );
        }

        public async Task<bool> UpdateStatusAsync(long bookingId, string status, string? note, CancellationToken ct)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, ct);
            if (booking == null) return false;

            booking.BookingStatus = Enum.Parse<BookingStatus>(status);
            booking.Note = note;
            //booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }
    }

}
