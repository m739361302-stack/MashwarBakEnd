using Application.DTOs.BookingDtos;
using Application.DTOs.CustomerDtos;
using Application.DTOs.Result;
using Application.Interfaces.IBooking;
using Application.Services.Security;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Application.Services.BookingsService
{
    public class CustomerBookingsService : ICustomerBookingsService
    {
        private readonly MashwarDbContext _db;
        public CustomerBookingsService(MashwarDbContext db) => _db = db;

        public async Task<ApiResult<long>> CreateBookingAsync(long customerUserId, CreateBookingDto dto, CancellationToken ct)
        {
            if (dto.TripId <= 0) return ApiResult<long>.Fail("TripId is required.");
            if (dto.PassengersCount <= 0) return ApiResult<long>.Fail("PassengersCount must be > 0.");

            var now = DateTime.UtcNow;

            // Trip must exist and be bookable
            var trip = await _db.Trips
                .Include(t => t.FromCity)
                .Include(t => t.ToCity)
                .Include(t => t.Driver)
                .ThenInclude(d => d.User) // لو موجودة
                .FirstOrDefaultAsync(t => t.Id == dto.TripId, ct);

            if (trip is null) return ApiResult<long>.NotFound("Trip not found.");

            if (!trip.IsActive) return ApiResult<long>.Fail("Trip is not active.");
            if (trip.DepartAt < now) return ApiResult<long>.Fail("Trip already departed.");
            if (!trip.Driver.IsAvailable) return ApiResult<long>.Fail("Driver is not available.");

            // Optional: require driver approved
            const int Approved = 1;
            //if (trip.Driver.ApprovalStatus != Approved)
            //    return ApiResult<int>.Fail("Driver is not approved.");

            // Prevent duplicate booking by same customer on same trip if active/pending
            var already = await _db.Bookings.AnyAsync(b =>
                b.TripId == dto.TripId &&
                b.CustomerUserId == (int)customerUserId &&
                (b.BookingStatus == BookingStatus.PendingDriverConfirm || b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Confirmed),
                ct);



            if (already) return ApiResult<long>.Fail("You already have an active booking for this trip.");

            // Prevent trip being booked by someone else if you want "one booking per trip"
            var tripTaken = await _db.Bookings.AnyAsync(b =>
                b.TripId == dto.TripId &&
                (b.BookingStatus == BookingStatus.PendingDriverConfirm || b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Confirmed),
                ct);

            if (tripTaken) return ApiResult<long>.Fail("Trip is already booked.");

            var booking = new Booking
            {
                TripId = trip.Id,
                DriverUserId = trip.DriverUserId,
                CustomerUserId = (int)customerUserId,
                PassengersCount = dto.PassengersCount,
                Note = dto.Note?.Trim(),
                BookingStatus = BookingStatus.PendingDriverConfirm,
                CreatedAt = now
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync(ct);

            return ApiResult<long>.Ok(booking.Id, "Booking created.");
        }

        public async Task<ApiResult<List<BookingListItemDto>>> GetMyBookingsAsync(long customerUserId, CancellationToken ct)
        {
            var q = _db.Bookings.AsNoTracking()
                .Where(b => b.CustomerUserId == (int)customerUserId)
                .Include(b => b.Trip).ThenInclude(t => t.FromCity)
                .Include(b => b.Trip).ThenInclude(t => t.ToCity)
                .Include(b => b.Driver).ThenInclude(d => d.User)
                .Include(b => b.Customer).ThenInclude(d => d.User)
                ;

            var list = await q
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BookingListItemDto(
                    b.Id,
                    b.BookingStatus,
                    b.TripId,
                    b.Trip.DepartAt,
                    b.Trip.Price,
                    b.Trip.FromCityId, b.Trip.FromCity.NameAr,
                    b.Trip.ToCityId, b.Trip.ToCity.NameAr,
                    b.DriverUserId,
                    b.Driver.User.FullName, // عدّلها إذا ما عندك User nav
                    b.Customer.User.FullName,
                    b.PassengersCount,
                    b.CreatedAt
                ))
                .ToListAsync(ct);

            return ApiResult<List<BookingListItemDto>>.Ok(list);
        }

        public async Task<ApiResult> CancelBookingAsync(long customerUserId, int bookingId, CancellationToken ct)
        {
            var booking = await _db.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerUserId == (int)customerUserId, ct);

            if (booking is null) return ApiResult.NotFound("Booking not found.");

            // يسمح الإلغاء فقط قبل التأكيد/الاكتمال/الرفض
            if (booking.BookingStatus is BookingStatus.RejectedByDriver or BookingStatus.Completed or BookingStatus.CancelledByCustomer)
                return ApiResult.Fail("Booking cannot be cancelled.");

            if (booking.BookingStatus == BookingStatus.Confirmed)
                return ApiResult.Fail("Cannot cancel a confirmed booking (handle refunds in payments module).");

            booking.BookingStatus = BookingStatus.CancelledByAdmin;
            //booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Booking cancelled.");
        }

        public async Task<ApiResult> ConfirmBookingAsync(long customerUserId, int bookingId, CancellationToken ct)
        {
            var booking = await _db.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.CustomerUserId == (int)customerUserId, ct);

            if (booking is null) return ApiResult.NotFound("Booking not found.");

            // في مرحلة 6 نخليها من Accepted -> Confirmed (تمهيد لمرحلة الدفع)
            if (booking.BookingStatus != BookingStatus.Confirmed)
                return ApiResult.Fail("Only accepted bookings can be confirmed.");

            booking.BookingStatus = BookingStatus.Confirmed;
            //booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Booking confirmed.");
        }

   
    }
}
