using Application.Interfaces.ICity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.Cities.CitiesDtos;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityService _service;
        public CitiesController(ICityService service) => _service = service;

        // GET /cities  => النشطة فقط
        [HttpGet("GitAllcities")]
        public async Task<IActionResult> GetActive(CancellationToken ct = default)
        {
            var res = await _service.GetPublicCitiesAsync(ct);
            return Ok(res.Data);
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin/all")]
        public async Task<ActionResult<List<AdminCityListItemDto>>> GetAll(CancellationToken ct)
          => Ok(await _service.GetAllAsync(ct));

        [HttpPost("create")]
        public async Task<ActionResult<AdminCityListItemDto>> Create([FromBody] AdminCreateCityRequest req, CancellationToken ct)
        {
            try
            {
                var created = await _service.CreateAsync(req, ct);
                return Ok(created);
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPut("update/{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] AdminUpdateCityRequest req, CancellationToken ct)
        {
            try
            {
                var ok = await _service.UpdateAsync(id, req, ct);
                if (!ok) return NotFound(new { message = "City not found" });
                return Ok(new { message = "Updated" });
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        }

        [HttpPatch("{id:int}/toggle-active")]
        public async Task<ActionResult> ToggleActive(int id, CancellationToken ct)
        {
            var ok = await _service.ToggleActiveAsync(id, ct);
            if (!ok) return NotFound(new { message = "City not found" });
            return Ok(new { message = "Toggled" });
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            var ok = await _service.DeleteAsync(id, ct);
            if (!ok) return NotFound(new { message = "City not found" });
            return Ok(new { message = "Deleted" });
        }

    }
}
