using Application.DTOs.BookingDtos;
using Application.DTOs.CustomerDtos;
using Application.DTOs.Result;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.IBooking
{
    public interface ICustomerBookingsService
    {
        Task<ApiResult<long>> CreateBookingAsync(long customerUserId, CreateBookingDto dto, CancellationToken ct);
        Task<ApiResult<List<BookingListItemDto>>> GetMyBookingsAsync(long customerUserId, CancellationToken ct);

        Task<ApiResult> CancelBookingAsync(long customerUserId, int bookingId, CancellationToken ct);

        // Confirm = سيتم استخدامها لاحقاً مع الدفع، لكن نخليها موجودة الآن كبنية
        Task<ApiResult> ConfirmBookingAsync(long customerUserId, int bookingId, CancellationToken ct);

    }

}
