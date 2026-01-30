using Application.Interfaces.IBooking;
using Application.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIs.Controllers
{
    //[Route("drivers/me/bookings")]
    //[ApiController]
    [Route("api/[controller]")]

    public class DriverBookingsController : ControllerBase
    {
        private readonly IDriverBookingsService _service;
        public DriverBookingsController(IDriverBookingsService service) => _service = service;

        [HttpGet("drivers/me/bookings")]
        public async Task<IActionResult> Get(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.GetDriverBookingsAsync(userId, ct);
            if (!res.Success) return NotFound(new { message = res.Message });
            return Ok(res.Data);
        }

        [HttpPost("{bookingId:int}/accept")]
        public async Task<IActionResult> Accept(int bookingId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.AcceptAsync(userId, bookingId, ct);

            if (!res.Success)
            {
                if ((res.Message ?? "").Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = res.Message });
                return Conflict(new { message = res.Message });
            }

            return NoContent();
        }

        [HttpPost("{bookingId:int}/reject")]
        public async Task<IActionResult> Reject(int bookingId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.RejectAsync(userId, bookingId, ct);

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
