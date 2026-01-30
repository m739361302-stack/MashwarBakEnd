using Application.DTOs.BookingDtos;
using Application.DTOs.Result;
using Application.Interfaces.IBooking;
using Domain.Entities;
using Domain.Enums;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.BookingsService
{
    public class DriverBookingsService : IDriverBookingsService
    {
        private readonly MashwarDbContext _db;
        public DriverBookingsService(MashwarDbContext db) => _db = db;

        private async Task<Driver?> GetDriverByUserIdAsync(long userId, CancellationToken ct)
            => await _db.Drivers.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.UserId == userId, ct);

        public async Task<ApiResult<List<BookingListItemDto>>> GetDriverBookingsAsync(long driverUserId, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(driverUserId, ct);
            if (driver is null) return ApiResult<List<BookingListItemDto>>.NotFound("Driver not found.");

            var q = _db.Bookings.AsNoTracking()
                .Where(b => b.DriverUserId == driver.UserId)
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
                    b.Driver.User.FullName,
                    b.Customer.User.FullName,
                    b.PassengersCount,
                    b.CreatedAt
                    

                ))
                .ToListAsync(ct);

            return ApiResult<List<BookingListItemDto>>.Ok(list);
        }

        public async Task<ApiResult> AcceptAsync(long driverUserId, int bookingId, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(driverUserId, ct);
            if (driver is null) return ApiResult.NotFound("Driver not found.");

            var booking = await _db.Bookings
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.DriverUserId == driver.UserId, ct);

            if (booking is null) return ApiResult.NotFound("Booking not found.");

            if (booking.BookingStatus != BookingStatus.PendingDriverConfirm)
                return ApiResult.Fail("Only pending bookings can be accepted.");

            // تأكد أن الرحلة ما زالت قابلة
            if (!booking.Trip.IsActive) return ApiResult.Fail("Trip is not active.");
            if (booking.Trip.DepartAt < DateTime.UtcNow) return ApiResult.Fail("Trip already departed.");

            booking.BookingStatus = BookingStatus.Confirmed;
           // booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Booking accepted.");
        }

        public async Task<ApiResult> RejectAsync(long driverUserId, int bookingId, CancellationToken ct)
        {
            var driver = await GetDriverByUserIdAsync(driverUserId, ct);
            if (driver is null) return ApiResult.NotFound("Driver not found.");

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.DriverUserId == driver.UserId, ct);

            if (booking is null) return ApiResult.NotFound("Booking not found.");

            if (booking.BookingStatus != BookingStatus.PendingDriverConfirm)
                return ApiResult.Fail("Only pending bookings can be rejected.");

            booking.BookingStatus = BookingStatus.RejectedByDriver;
            //booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            return ApiResult.Ok("Booking rejected.");
        }
    }
}
