using Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.Trips.AdminTripDtos;

namespace APIs.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]
    //[ApiController]
    public class AdminTripsController : ControllerBase
    {
        private readonly AdminTripsService _svc;

        public AdminTripsController(AdminTripsService svc)
        {
            _svc = svc;
        }

        [HttpGet("getTrips")]
        public async Task<ActionResult<List<AdminTripListItemDto>>> Get(CancellationToken ct)
        {
            var list = await _svc.GetTripsAsync(ct);
            return Ok(list);
        }

        [HttpGet("{tripId:long}")]
        public async Task<ActionResult<AdminTripDetailsDto>> GetOne(long tripId, CancellationToken ct)
        {
            var dto = await _svc.GetTripDetailsAsync(tripId, ct);
            if (dto == null) return NotFound(new { message = "Trip not found" });
            return Ok(dto);
        }

        [HttpPut("{tripId:long}/active")]
        public async Task<ActionResult> SetActive(long tripId, [FromBody] AdminSetTripActiveRequest req, CancellationToken ct)
        {
            try
            {
                var ok = await _svc.SetTripActiveAsync(tripId, req.IsActive, ct);
                if (!ok) return NotFound(new { message = "Trip not found" });
                return Ok(new { message = "Updated" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message }); // 409
            }
        }
    }
}
