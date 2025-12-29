using Application.DTOs.Auth;
using Application.DTOs.DriverApprovalDtos;
using Application.Interfaces.Admin;
using Application.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.AdminOnly)]
    public class AdminDriversController : ControllerBase
    {
        private readonly IAdminDriverApprovalsService _svc;

        public AdminDriversController(IAdminDriverApprovalsService svc)
        {
            _svc = svc;
        }

        [HttpGet("approvals")]
        public async Task<ActionResult<PagedResultDto<DriverApprovalListItemDto>>> GetApprovals(
            [FromQuery] string status = "pending",
            [FromQuery] int? cityId = null,
            [FromQuery] string? q = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20
        )
        {
            var res = await _svc.GetApprovalsAsync(status, cityId, q, page, pageSize);
            return Ok(res);
        }

        [HttpPost("{driverUserId:long}/approve")]
        public async Task<ActionResult<ApiOkDto>> Approve(long driverUserId, [FromBody] ApproveDriverRequestDto dto)
        {
            var adminIdStr = User.FindFirstValue("uid");
            if (!long.TryParse(adminIdStr, out var adminId))
                return Unauthorized(new { message = "Invalid token" });

            try
            {
                var res = await _svc.ApproveAsync(driverUserId, adminId, dto.Note);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{driverUserId:long}/reject")]
        public async Task<ActionResult<ApiOkDto>> Reject(long driverUserId, [FromBody] RejectDriverRequestDto dto)
        {
            var adminIdStr = User.FindFirstValue("uid");
            if (!long.TryParse(adminIdStr, out var adminId))
                return Unauthorized(new { message = "Invalid token" });

            try
            {
                var res = await _svc.RejectAsync(driverUserId, adminId, dto.Reason);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
