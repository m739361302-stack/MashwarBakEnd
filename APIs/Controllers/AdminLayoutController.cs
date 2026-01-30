using Application.DTOs.Admin;
using Application.Services.Admin;
using Application.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIs.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("api/[controller]")]
  
    public class AdminLayoutController : ControllerBase
    {
        private readonly AdminLayoutService _service;

        public AdminLayoutController(AdminLayoutService service)
        {
            _service = service;
        }

        [HttpGet("getAdminLayout")]
        public async Task<ActionResult<AdminLayoutDto>> GetAdminLayout(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var result = await _service.GetAdminLayoutAsync(ct, userId);
            return Ok(result);
        }
    }
}
