using Application.DTOs.Admin;
using Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Composition;
using static Application.DTOs.Admin.AdminReportsDtos;

namespace APIs.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]

    public class AdminReportsController : ControllerBase
    {
        private readonly AdminReportsService _service;

        public AdminReportsController(AdminReportsService service)
        {
            _service = service;
        }

        [HttpGet("admin/reports")]
        public async Task<ActionResult<AdminReportsResponse>> Get([FromQuery] AdminReportsQuery q, CancellationToken ct)
        {
            var result = await _service.GetAdminReportsAsync(q, ct);
            return Ok(result);
        }

        [HttpPost("export-csv")]
        public async Task<IActionResult> ExportCsv([FromBody] AdminReportsFilterDto filters, CancellationToken ct)
        {
            var bytes = await _service.ExportCsvAsync(filters, ct);

            var fileName = $"reports_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }
    }
}
