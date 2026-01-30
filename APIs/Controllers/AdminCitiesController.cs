using Application.Interfaces.ICity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.Cities.CitiesDtos;

namespace APIs.Controllers
{

    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("admin/cities")]
    public class AdminCitiesController : ControllerBase
    {
        private readonly ICityService _service;
        public AdminCitiesController(ICityService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = true, CancellationToken ct = default)
        {
            var res = await _service.GetAdminCitiesAsync(includeInactive, ct);
            return Ok(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CityCreateDto dto, CancellationToken ct = default)
        {
            var res = await _service.CreateAsync(dto, ct);
            if (!res.Success)
                return Conflict(new { message = res.Message });

            return StatusCode(201, res.Data);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CityUpdateDto dto, CancellationToken ct = default)
        {
            var res = await _service.UpdateAsync(id, dto, ct);

            if (!res.Success)
            {
                if (res.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(new { message = res.Message });

                return Conflict(new { message = res.Message });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> SoftDelete(int id, CancellationToken ct = default)
        {
            var res = await _service.SoftDeleteAsync(id, ct);

            if (!res.Success)
            {
                if (res.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(new { message = res.Message });

                return Conflict(new { message = res.Message });
            }

            return NoContent();
        }
    }
}
