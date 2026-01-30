using Application.Interfaces.IDriver;
using Application.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.DriverDto.Availability;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    public class DriverAvailabilityController : ControllerBase
    {
        private readonly IDriverAvailabilityService _service;
        public DriverAvailabilityController(IDriverAvailabilityService service) => _service = service;

        // GET /drivers/me/availability
        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.GetDriverAvailabilityAsync(userId, ct);
            if (!res.Success) return NotFound(new { message = res.Message });
            return Ok(res.Data);
        }

        // GET /drivers/me/availability/slots
        [HttpGet("availability/slots")]
        public async Task<IActionResult> GetSlots(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.GetSlotsAsync(userId, ct);
            if (!res.Success) return NotFound(new { message = res.Message });
            return Ok(res.Data);
        }

        // POST /drivers/me/availability  (Create slot)
        [HttpPost("availabilityCreate")]
        public async Task<IActionResult> CreateSlot([FromBody] CreateAvailabilitySlotDto dto, CancellationToken ct)
        {




            var userId = User.GetUserId();
            var res = await _service.CreateSlotAsync(userId, dto, ct);
            if (!res.Success) return Conflict(new { message = res.Message });

            return StatusCode(201, new { id = res.Data, message = res.Message });
        }

        // DELETE /drivers/me/availability/{slotId}
        [HttpDelete("availabilityDelete/{slotId:int}")]
        public async Task<IActionResult> DeleteSlot(int slotId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.DeleteSlotAsync(userId, slotId, ct);

            if (!res.Success)
            {
                if ((res.Message ?? "").Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = res.Message });

                return Conflict(new { message = res.Message });
            }
            return NoContent();
        }

        // PUT /drivers/me/availability/{slotId} (اختياري)
        [HttpPut("availabilityUpdate/{slotId:int}")]
        public async Task<IActionResult> UpdateSlot(int slotId, [FromBody] UpdateAvailabilitySlotDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.UpdateSlotAsync(userId, slotId, dto, ct);

            if (!res.Success)
            {
                if ((res.Message ?? "").Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = res.Message });

                return Conflict(new { message = res.Message });
            }
            return NoContent();
        }

        // POST /drivers/me/set-availability
        [HttpPost("set-availability")]
        public async Task<IActionResult> SetAvailability([FromBody] SetDriverAvailabilityDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.SetDriverAvailabilityAsync(userId, dto, ct);

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
