using Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.Admin.dashboardDtos;

namespace APIs.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]
    //[ApiController]
    public class AdminDashboardController : ControllerBase
    {
        private readonly AdminDashboardService _service;

        public AdminDashboardController(AdminDashboardService service)
        {
            _service = service;
        }



        [HttpGet("getAdminDashboard")]
        public async Task<ActionResult<AdminDashboardDto>> GetAdminDashboard(CancellationToken ct)
        {
            var result = await _service.GetAdminDashboardAsync(ct);
            return Ok(result);
        }
    }
}
