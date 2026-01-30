using Application.Interfaces.ICity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.CityRoutes.CityRoutesDtos;

namespace APIs.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    //[ApiController]
    [Route("admin/city-routes")]
    public class AdminCityRoutesController : ControllerBase
    {
        private readonly ICityRouteService _service;
        public AdminCityRoutesController(ICityRouteService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = true, CancellationToken ct = default)
        {
            var res = await _service.GetAdminRoutesAsync(includeInactive, ct);
            return Ok(res.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CityRouteCreateDto dto, CancellationToken ct = default)
        {
            var res = await _service.CreateAsync(dto, ct);
            if (!res.Success) return Conflict(new { message = res.Message });

            return StatusCode(201, new { id = res.Data, message = res.Message });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CityRouteUpdateDto dto, CancellationToken ct = default)
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
    }
}
