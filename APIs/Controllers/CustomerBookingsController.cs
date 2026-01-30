using Application.DTOs.BookingDtos;
using Application.DTOs.CustomerDtos;
using Application.Interfaces.IBooking;
using Application.Services.Security;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    //[Route("customers/me/bookings")]
    //[ApiController]
  
    public class CustomerBookingsController : ControllerBase
    {
        private readonly ICustomerBookingsService _service;
        public CustomerBookingsController(ICustomerBookingsService service) => _service = service;

        [HttpGet("MyBookings")]
        public async Task<IActionResult> MyBookings(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.GetMyBookingsAsync(userId, ct);
            if (!res.Success) return NotFound(new { message = res.Message });
            return Ok(res.Data);
        }

        [HttpPost("me/bookings/create")]
        public async Task<IActionResult> Create([FromBody] CreateBookingDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.CreateBookingAsync(userId, dto, ct);
            if (!res.Success) return Conflict(new { message = res.Message });

            return StatusCode(201, new { id = res.Data, message = res.Message });
        }

        [HttpPost("{bookingId:int}/cancel")]
        public async Task<IActionResult> Cancel(int bookingId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.CancelBookingAsync(userId, bookingId, ct);

            if (!res.Success)
            {
                if ((res.Message ?? "").Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = res.Message });
                return Conflict(new { message = res.Message });
            }

            return NoContent();
        }

        // confirm (تمهيد لمرحلة الدفع)
        [HttpPost("{bookingId:int}/confirm")]
        public async Task<IActionResult> Confirm(int bookingId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.ConfirmBookingAsync(userId, bookingId, ct);

            if (!res.Success)
            {
                if ((res.Message ?? "").Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = res.Message });
                return Conflict(new { message = res.Message });
            }

            return NoContent();
        }

 
    }
}
