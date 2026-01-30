using Application.DTOs.BookingDtos;
using Application.DTOs.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.IBooking
{
    public interface IDriverBookingsService
    {
        Task<ApiResult<List<BookingListItemDto>>> GetDriverBookingsAsync(long driverUserId, CancellationToken ct);
        Task<ApiResult> AcceptAsync(long driverUserId, int bookingId, CancellationToken ct);
        Task<ApiResult> RejectAsync(long driverUserId, int bookingId, CancellationToken ct);
    }

}
