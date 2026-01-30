using Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.BookingDtos.AdminBookingDtos;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminBookingsController : ControllerBase
    {
        private readonly AdminBookingsService _service;

        public AdminBookingsController(AdminBookingsService service)
        {
            _service = service;
        }
        //[AllowAnonymous]
        [HttpGet("admin/bookings")]
        public async Task<ActionResult<List<AdminBookingListItemDto>>> GetAll(CancellationToken ct)
        {
            return Ok(await _service.GetAllAsync(ct));
        }

        [HttpGet("{bookingId:long}")]
        public async Task<ActionResult<AdminBookingDetailsDto>> GetOne(long bookingId, CancellationToken ct)
        {
            var dto = await _service.GetByIdAsync(bookingId, ct);
            if (dto == null) return NotFound(new { message = "Booking not found" });
            return Ok(dto);
        }

        [HttpPut("{bookingId:long}/status")]
        public async Task<ActionResult> UpdateStatus(
            long bookingId,
            [FromBody] AdminUpdateBookingStatusRequest req,
            CancellationToken ct)
        {
            var ok = await _service.UpdateStatusAsync(bookingId, req.Status, req.Note, ct);
            if (!ok) return NotFound(new { message = "Booking not found" });

            return Ok(new { message = "Updated" });
        }
    }

}
