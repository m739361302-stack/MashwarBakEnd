using Application.DTOs.SearchTripsDto;
using Application.DTOs.Trips;
using Application.Interfaces.TripsSearch;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsSearchController : ControllerBase
    {
        private readonly ITripsSearchService _service;
        public TripsSearchController(ITripsSearchService service) => _service = service;

        // GET /trips/search?fromCityId=1&toCityId=2&date=2026-01-20&requireAvailabilitySlot=false
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] int fromCityId,
            [FromQuery] int toCityId,
            [FromQuery] DateTime? date,
            [FromQuery] bool requireAvailabilitySlot = false,
            CancellationToken ct = default)
        {
            // العميل ممكن يكون Guest، لذلك userId اختياري
            long? userId = null;
            // إذا عندك Auth اختياري وتبغى تقرأ userId لو موجود:
            // if (User?.Identity?.IsAuthenticated == true) userId = User.GetUserId();

            var q = new SearchTripsQueryDto(fromCityId, toCityId, date);
            var res = await _service.SearchAsync(userId, q, requireAvailabilitySlot, ct);

            if (!res.Success)
                return BadRequest(new { message = res.Message });

            return Ok(res.Data);
        }

        [HttpGet("topTrips")]
        public async Task<ActionResult<List<TopTripDto>>> GetTopTrips(
            [FromQuery] int? fromCityId,
            [FromQuery] int? toCityId,
            [FromQuery] DateTime? date,
            [FromQuery] int limit = 3,
            CancellationToken ct = default)
        {
            var result = await _service.GetTopTripsAsync(
                fromCityId, toCityId, date, limit, ct);

            return Ok(result);
        }

    }
}
