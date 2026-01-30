using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIs.Controllers
{
    using Application.Interfaces.IDriver;
    using Application.Services.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using static Application.DTOs.Trips.TripsDto;

    [Route("api/[controller]")]
    //[Route("drivers/me/trips")]

    public class DriverTripsController : ControllerBase
    {
        private readonly IDriverTripsService _service;
        public DriverTripsController(IDriverTripsService service) => _service = service;

        // GET /drivers/me/trips?status=active|inactive&from=&to=&dateFrom=
        [HttpGet("GetMyTrips")]
        public async Task<IActionResult> GetMyTrips(
            [FromQuery] string? status,
            [FromQuery] int? from,
            [FromQuery] int? to,
            [FromQuery] DateTime? dateFrom,
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.GetMyTripsAsync(userId, status, from, to, dateFrom, ct);
            if (!res.Success) return NotFound(new { message = res.Message });
            return Ok(res.Data);
        }

        // POST /drivers/me/trips

        [HttpPost("drivers/me/trips/create")]

        public async Task<IActionResult> Create([FromBody] CreateTripDto dto, CancellationToken ct)
        {
            var authHeader = Request.Headers.Authorization.ToString();
            var userId = User.GetUserId();
            var res = await _service.CreateTripAsync(userId, dto, ct);
            if (!res.Success) return Conflict(new { message = res.Message });

            return StatusCode(201, new { id = res.Data, message = res.Message });
        }

        // PUT /drivers/me/trips/{tripId}
        [HttpPut("drivers/me/trips/update/{tripId:int}")]
        public async Task<IActionResult> Update(int tripId, [FromBody] UpdateTripDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.UpdateTripAsync(userId, tripId, dto, ct);

            if (!res.Success)
            {
                if ((res.Message ?? "").Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = res.Message });

                return Conflict(new { message = res.Message });
            }

            return NoContent();
        }

        // POST /drivers/me/trips/{tripId}/activate
        [HttpPost("drivers/me/trips/{tripId:int}/activate")]
        public async Task<IActionResult> Activate(int tripId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.ActivateTripAsync(userId, tripId, ct);

            if (!res.Success)
            {
                if ((res.Message ?? "").Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = res.Message });

                return Conflict(new { message = res.Message });
            }

            return NoContent();
        }

        // POST /drivers/me/trips/{tripId}/deactivate
        [HttpPost("drivers/me/trips/{tripId:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int tripId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var res = await _service.DeactivateTripAsync(userId, tripId, ct);

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
